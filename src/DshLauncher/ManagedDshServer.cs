using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
using DshLauncher.Core;

namespace DshLauncher;

internal sealed partial class ManagedDshServer : IAsyncDisposable
{
    private readonly AppPaths paths;
    private readonly Log log;
    private readonly LauncherStateStore stateStore;
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly object lifecycleSync = new();
    private Process? process;
    private CancellationTokenSource? monitorCancellation;
    private Task? monitorTask;
    private long openGeneration;
    private long? pendingShutdownGeneration;

    public ManagedDshServer(AppPaths paths, Log log)
    {
        this.paths = paths;
        this.log = log;
        stateStore = new LauncherStateStore(paths, log);
    }

    public event EventHandler? Stopped;

    public bool IsRunning
    {
        get
        {
            Process? current = process;
            if (current is null) return false;
            try { return !current.HasExited; }
            catch { return false; }
        }
    }

    public int Port { get; private set; }
    public string? AccessUrl { get; private set; }
    public DshInstall? Install { get; private set; }

    public async Task StartAsync(DshInstall install, CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (IsRunning) return;

            Install = install;
            Port = PortAllocator.Select();
            AccessUrl = null;
            PrepareStatusPlugin();
            TryDelete(paths.TaskState);

            var startInfo = new ProcessStartInfo
            {
                FileName = install.Node.NodePath,
                WorkingDirectory = install.PackageDirectory,
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
            };
            foreach (string argument in new[]
            {
                install.EntryPoint,
                "--profile", "web",
                "--patch", paths.Patch,
                "--port", Port.ToString(System.Globalization.CultureInfo.InvariantCulture),
                "--no-open",
            })
            {
                startInfo.ArgumentList.Add(argument);
            }
            startInfo.Environment["DSH_LAUNCHER_STATUS_FILE"] = paths.TaskState;

            var started = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
            started.OutputDataReceived += OnServerOutput;
            started.ErrorDataReceived += OnServerError;
            started.Exited += OnServerExited;
            if (!started.Start()) throw new InvalidOperationException("Could not start the global DSH process.");
            process = started;
            started.BeginOutputReadLine();
            started.BeginErrorReadLine();

            await WriteStateAsync("starting", cancellationToken);
            await WaitUntilReadyAsync(started, cancellationToken);
            VerifyLoopbackBinding();
            await WriteStateAsync("ready", cancellationToken);

            monitorCancellation = new CancellationTokenSource();
            monitorTask = MonitorLifecycleAsync(started, monitorCancellation.Token);
            log.Write($"Started npm global DSH {install.Version} on 127.0.0.1:{Port}, PID {started.Id}.");
        }
        catch
        {
            if (process is not null) ProcessRunner.TryKillTree(process);
            process = null;
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task AdoptAsync(
        DshInstall install,
        RecoveredDshSession session,
        CancellationToken cancellationToken = default)
    {
        await gate.WaitAsync(cancellationToken);
        try
        {
            if (IsRunning) return;

            Process adopted = Process.GetProcessById(session.ProcessId);
            if (adopted.HasExited) throw new InvalidOperationException("The recovered DSH process has already stopped.");
            adopted.EnableRaisingEvents = true;
            adopted.Exited += OnServerExited;
            process = adopted;
            Install = install;
            Port = session.Port;
            AccessUrl = session.AccessUrl;
            await WaitUntilReadyAsync(adopted, cancellationToken);
            VerifyLoopbackBinding();
            await WriteStateAsync("ready", cancellationToken);

            monitorCancellation = new CancellationTokenSource();
            monitorTask = MonitorLifecycleAsync(adopted, monitorCancellation.Token);
            log.Write($"Securely recovered npm global DSH {install.Version} on 127.0.0.1:{Port}, PID {adopted.Id}.");
        }
        catch
        {
            process = null;
            throw;
        }
        finally
        {
            gate.Release();
        }
    }

    public void RecordOpen()
    {
        lock (lifecycleSync)
        {
            openGeneration++;
            pendingShutdownGeneration = null;
        }
    }

    public async Task StopAsync()
    {
        await gate.WaitAsync();
        try
        {
            monitorCancellation?.Cancel();
            Process? current = process;
            if (current is null) return;
            try
            {
                if (!current.HasExited)
                {
                    current.Kill(entireProcessTree: true);
                    using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(8));
                    await current.WaitForExitAsync(timeout.Token);
                }
            }
            catch (OperationCanceledException)
            {
                ProcessRunner.TryKillTree(current);
            }
            finally
            {
                process = null;
                TryDelete(paths.State);
                TryDelete(paths.TaskState);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async ValueTask DisposeAsync()
    {
        await StopAsync();
        monitorCancellation?.Dispose();
        gate.Dispose();
    }

    private void PrepareStatusPlugin()
    {
        using Stream source = typeof(ManagedDshServer).Assembly.GetManifestResourceStream(
            "DshLauncher.Resources.launcher-status-plugin.mjs")
            ?? throw new InvalidDataException("Embedded DSH status plugin is missing.");
        using (var destination = new FileStream(paths.Plugin, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            source.CopyTo(destination);
        }
        string pluginUri = new Uri(paths.Plugin).AbsoluteUri.Replace("'", "''", StringComparison.Ordinal);
        string statusPath = paths.TaskState.Replace("'", "''", StringComparison.Ordinal);
        string patch = $"""
            - insert:
                - id: dsh-launcher-status
                  name: '{pluginUri}'
                  inject: [agents]
                  config:
                    path: '{statusPath}'
            """;
        File.WriteAllText(paths.Patch, patch.ReplaceLineEndings("\n"));
    }

    private async Task WaitUntilReadyAsync(Process started, CancellationToken cancellationToken)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        DateTimeOffset deadline = DateTimeOffset.UtcNow.AddSeconds(40);
        while (DateTimeOffset.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (started.HasExited)
            {
                throw new InvalidOperationException($"DSH exited before becoming ready (code {started.ExitCode}).");
            }

            try
            {
                string readinessUrl = AccessUrl ?? $"http://127.0.0.1:{Port}/";
                using HttpResponseMessage response = await client.GetAsync(
                    readinessUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    AccessUrl ??= readinessUrl;
                    return;
                }
            }
            catch (HttpRequestException)
            {
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
            }
            await Task.Delay(200, cancellationToken);
        }

        throw new TimeoutException("DSH did not listen on the selected port within 40 seconds.");
    }

    private void VerifyLoopbackBinding()
    {
        IPEndPoint? listener = IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpListeners()
            .FirstOrDefault(endpoint => endpoint.Port == Port);
        if (listener is null || !IPAddress.IsLoopback(listener.Address))
        {
            throw new InvalidOperationException("DSH did not bind exclusively to a loopback address; startup was stopped.");
        }
    }

    private async Task MonitorLifecycleAsync(Process started, CancellationToken cancellationToken)
    {
        bool seenClient = false;
        DateTimeOffset? noClientSince = null;
        try
        {
            while (!cancellationToken.IsCancellationRequested && !started.HasExited)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), cancellationToken);
                int clients = PortAllocator.CountLoopbackClients(Port);
                if (clients > 0)
                {
                    seenClient = true;
                    noClientSince = null;
                    lock (lifecycleSync) pendingShutdownGeneration = null;
                    continue;
                }
                if (!seenClient) continue;

                noClientSince ??= DateTimeOffset.UtcNow;
                if (DateTimeOffset.UtcNow - noClientSince < TimeSpan.FromSeconds(12)) continue;

                int running = TaskStateReader.ReadRunningCount(paths.TaskState, started.Id);
                if (running < 0) continue;
                if (running == 0)
                {
                    long generation;
                    long? pending;
                    lock (lifecycleSync)
                    {
                        generation = openGeneration;
                        pending = pendingShutdownGeneration;
                    }

                    if (pending is null || pending == generation)
                    {
                        log.Write("Browser closed and no DSH task is running; stopping the managed server.");
                        await StopAsync();
                        return;
                    }
                    noClientSince = null;
                    continue;
                }

                lock (lifecycleSync)
                {
                    pendingShutdownGeneration ??= openGeneration;
                }
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            log.Write($"Lifecycle monitor kept server alive after an error: {exception.Message}");
        }
    }

    private async Task WriteStateAsync(string status, CancellationToken cancellationToken)
    {
        await stateStore.WriteAsync(status, process, Port, Install, AccessUrl, cancellationToken);
    }

    private void OnServerOutput(object sender, DataReceivedEventArgs eventArgs)
    {
        if (string.IsNullOrWhiteSpace(eventArgs.Data)) return;
        Match match = LocalUrlRegex().Match(eventArgs.Data);
        if (match.Success && Uri.TryCreate(match.Value, UriKind.Absolute, out Uri? uri) &&
            uri.Host == "127.0.0.1" && uri.Port == Port)
        {
            AccessUrl = uri.AbsoluteUri;
        }
        log.Write($"DSH: {eventArgs.Data}");
    }

    private void OnServerError(object sender, DataReceivedEventArgs eventArgs)
    {
        if (!string.IsNullOrWhiteSpace(eventArgs.Data)) log.Write($"DSH error: {eventArgs.Data}");
    }

    private void OnServerExited(object? sender, EventArgs eventArgs)
    {
        TryDelete(paths.State);
        TryDelete(paths.TaskState);
        Stopped?.Invoke(this, EventArgs.Empty);
    }

    private static void TryDelete(string path)
    {
        try { File.Delete(path); }
        catch { }
    }

    [GeneratedRegex(@"https?://127\.0\.0\.1:\d+(?:/[^\s]*)?", RegexOptions.IgnoreCase)]
    private static partial Regex LocalUrlRegex();
}
