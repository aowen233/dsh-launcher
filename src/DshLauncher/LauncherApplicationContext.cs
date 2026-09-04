using System.Diagnostics;
using DshLauncher.Core;

namespace DshLauncher;

internal sealed class LauncherApplicationContext : ApplicationContext
{
    private readonly AppPaths paths = new();
    private readonly GlobalDshLocator locator = new();
    private readonly DependencyManifest manifest;
    private readonly Log log;
    private readonly ManagedDshServer server;
    private readonly NpmManager npm;
    private readonly DshUpdateChecker updateChecker;
    private readonly LauncherStateStore stateStore;
    private readonly PrerequisiteInstaller prerequisites;
    private readonly NotifyIcon tray;
    private readonly Control dispatcher = new();
    private readonly SemaphoreSlim operationGate = new(1, 1);
    private readonly SingleInstanceServer singleInstance;
    private bool busy;
    private bool intentionalStop;
    private bool disposed;

    public LauncherApplicationContext(string[] args)
    {
        manifest = DependencyManifest.Load();
        log = new Log(paths);
        server = new ManagedDshServer(paths, log);
        npm = new NpmManager(manifest, locator, log);
        updateChecker = new DshUpdateChecker(npm, paths, log);
        stateStore = new LauncherStateStore(paths, log);
        prerequisites = new PrerequisiteInstaller(manifest, new VerifiedDownloader(), paths, log);
        server.Stopped += OnServerStopped;

        dispatcher.CreateControl();
        tray = new NotifyIcon
        {
            Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath) ?? SystemIcons.Application,
            Text = "DSH Launcher",
            Visible = true,
            ContextMenuStrip = CreateMenu(),
        };
        tray.DoubleClick += async (_, _) => await ExecuteCommandAsync("open");

        singleInstance = new SingleInstanceServer(command => InvokeOnUiAsync(() => ExecuteCommandAsync(command)));

        var startupTimer = new System.Windows.Forms.Timer { Interval = 50 };
        startupTimer.Tick += async (_, _) =>
        {
            startupTimer.Stop();
            startupTimer.Dispose();
            await ExecuteCommandAsync(args.Length == 0 ? "open" : string.Join(' ', args));
        };
        startupTimer.Start();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && !disposed)
        {
            disposed = true;
            tray.Visible = false;
            tray.Dispose();
            singleInstance.Dispose();
            server.DisposeAsync().AsTask().GetAwaiter().GetResult();
            dispatcher.Dispose();
            operationGate.Dispose();
        }
        base.Dispose(disposing);
    }

    private ContextMenuStrip CreateMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("打开 DSH", null, async (_, _) => await ExecuteCommandAsync("open"));
        menu.Items.Add("检查并更新 DSH", null, async (_, _) => await ExecuteCommandAsync("update"));
        menu.Items.Add("安装/更新鲸鱼娘插件", null, async (_, _) => await ExecuteCommandAsync("install-whale"));
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("停止 DSH", null, async (_, _) => await ExecuteCommandAsync("stop"));
        menu.Items.Add("退出启动器", null, async (_, _) =>
        {
            intentionalStop = true;
            await server.StopAsync();
            ExitThread();
        });
        return menu;
    }

    private async Task ExecuteCommandAsync(string command)
    {
        await operationGate.WaitAsync();

        busy = true;
        try
        {
            string normalized = command.Trim().TrimStart('-').ToLowerInvariant();
            switch (normalized)
            {
                case "stop":
                    intentionalStop = true;
                    await server.StopAsync();
                    await StopExternalGlobalDshAsync(confirm: true);
                    ExitThread();
                    break;
                case "update":
                    await UpdateDshAsync(force: true);
                    break;
                case "update-confirmed":
                    await UpdateDshAsync(force: false);
                    break;
                case "install-whale":
                    await InstallWhaleAsync();
                    break;
                case "tray":
                    break;
                default:
                    await StartOrOpenAsync();
                    break;
            }
        }
        catch (OperationCanceledException)
        {
            log.Write("Operation cancelled.");
        }
        catch (Exception exception)
        {
            log.Write($"Operation failed: {exception}");
            DialogService.Error(
                "操作没有完成",
                $"{exception.Message}\r\n\r\n诊断日志：{paths.Logs}");
        }
        finally
        {
            busy = false;
            operationGate.Release();
        }
    }

    private async Task StartOrOpenAsync()
    {
        if (server.IsRunning)
        {
            OpenManagedServer();
            return;
        }

        NodeInstall node = await EnsureNodeAsync();
        DshInstall? dsh = await locator.FindDshAsync();
        if (dsh is null)
        {
            if (!DialogService.Confirm(
                    "需要安装 DSH",
                    "没有检测到 npm 全局版 DSH。是否现在自动安装？"))
            {
                throw new OperationCanceledException();
            }
            await npm.InstallOrUpdateDshAsync(node);
            dsh = await locator.FindDshAsync()
                ?? throw new InvalidOperationException("DSH installation completed but the global package could not be located.");
        }

        IReadOnlyList<GlobalDshProcess> existing = GlobalDshProcessInspector.Find(dsh);
        RecoveredDshSession? recovered = stateStore.TryRecover(dsh, existing);
        if (recovered is not null)
        {
            await server.AdoptAsync(dsh, recovered);
            OpenManagedServer();
            ShowBalloon($"已安全恢复并打开 npm 全局版 DSH {dsh.Version}。", ToolTipIcon.Info);
            _ = CheckForUpdateAsync(dsh);
            return;
        }

        GlobalDshProcess? reachable = existing.FirstOrDefault(item => item.Port is > 0 and <= 65535);
        if (reachable is not null)
        {
            BrowserService.Open($"http://127.0.0.1:{reachable.Port}/");
            ShowBalloon($"已打开正在运行的 npm 全局版 DSH {dsh.Version}。该进程不是由本次启动器创建，因此不会自动关闭。", ToolTipIcon.Info);
            _ = CheckForUpdateAsync(dsh);
            return;
        }

        bool preferredBusy = !PortAllocator.IsFree(PortPolicy.PreferredPort);
        await server.StartAsync(dsh);
        if (preferredBusy && server.Port != PortPolicy.PreferredPort)
        {
            DialogService.Information(
                "已自动更换端口",
                $"端口 {PortPolicy.PreferredPort} 已被其他程序占用，DSH 已安全改用端口 {server.Port}。");
        }
        OpenManagedServer();
        ShowBalloon($"npm 全局版 DSH {dsh.Version} 已启动。", ToolTipIcon.Info);
        _ = CheckForUpdateAsync(dsh);
    }

    private async Task<NodeInstall> EnsureNodeAsync()
    {
        NodeInstall? node = await locator.FindNodeAsync();
        if (node is not null && NodeVersionPolicy.IsSupported(node.Version)) return node;

        string reason = node is null
            ? "没有检测到 Node.js。"
            : $"当前 Node.js {node.Version} 不受 DSH 支持。";
        if (!DialogService.Confirm(
                "需要 Node.js",
                $"{reason}\r\n\r\n是否安装经过 SHA-256 校验的官方 Node.js {manifest.Node.Version} LTS？此步骤需要管理员确认。"))
        {
            throw new OperationCanceledException();
        }

        await prerequisites.InstallNodeAsync();
        locator.Invalidate();
        return await locator.FindNodeAsync()
            ?? throw new InvalidOperationException("Node.js installation completed but node.exe could not be located.");
    }

    private void OpenManagedServer()
    {
        string url = server.AccessUrl ?? $"http://127.0.0.1:{server.Port}/";
        server.RecordOpen();
        BrowserService.Open(url);
    }

    private async Task CheckForUpdateAsync(DshInstall dsh)
    {
        try
        {
            SemanticVersion? latest = await updateChecker.GetLatestAsync(dsh.Node);
            if (latest is null || latest.CompareTo(dsh.Version) <= 0) return;

            await InvokeOnUiAsync(async () =>
            {
                if (DialogService.Confirm(
                        "发现 DSH 更新",
                        $"当前版本：{dsh.Version}\r\n最新版本：{latest}\r\n\r\n是否停止当前任务和服务器并立即更新？"))
                {
                    await ExecuteCommandAsync("update-confirmed");
                }
            });
        }
        catch (Exception exception)
        {
            log.Write($"Background update check failed without interrupting startup: {exception.Message}");
        }
    }

    private async Task UpdateDshAsync(bool force)
    {
        NodeInstall node = await EnsureNodeAsync();
        DshInstall? current = await locator.FindDshAsync();
        if (current is not null && force)
        {
            if (!DialogService.Confirm(
                    "更新 npm 全局版 DSH",
                    $"将停止当前任务和服务器，然后安装最新版。\r\n当前版本：{current.Version}\r\n\r\n是否继续？",
                    warning: true))
            {
                throw new OperationCanceledException();
            }
        }

        intentionalStop = true;
        try
        {
            await server.StopAsync();
            if (current is not null) await StopExternalGlobalDshAsync(confirm: false, current);
            await npm.InstallOrUpdateDshAsync(node);
        }
        finally
        {
            intentionalStop = false;
        }

        DshInstall updated = await locator.FindDshAsync()
            ?? throw new InvalidOperationException("Updated DSH could not be located.");
        await updateChecker.RecordInstalledAsync(updated.Version);
        DialogService.Information("DSH 更新完成", $"当前版本：{updated.Version}");
        await server.StartAsync(updated);
        OpenManagedServer();
    }

    private async Task InstallWhaleAsync()
    {
        DshInstall dsh = await locator.FindDshAsync()
            ?? throw new InvalidOperationException("请先安装 npm 全局版 DSH。");
        WhaleDependency whale = manifest.WhaleWidget;
        if (!DialogService.Confirm(
                "安装第三方鲸鱼娘插件",
                $"来源：{whale.Author} 的官方 GitHub 仓库\r\n" +
                $"版本：{whale.Version}\r\n固定提交：{whale.Commit[..12]}\r\n许可证：MIT\r\n\r\n" +
                "该插件会读取 DSH 提供的 DeepSeek API 凭据以查询余额。是否继续？"))
        {
            throw new OperationCanceledException();
        }

        bool restart = server.IsRunning;
        intentionalStop = true;
        try
        {
            await server.StopAsync();
            await npm.InstallWhaleWidgetAsync(dsh);
        }
        finally
        {
            intentionalStop = false;
        }
        DialogService.Information(
            "鲸鱼娘插件安装完成",
            "版权归 MeteorNOX，许可证为 MIT。");
        if (restart)
        {
            await server.StartAsync(dsh);
            OpenManagedServer();
        }
    }

    private async Task StopExternalGlobalDshAsync(bool confirm, DshInstall? install = null)
    {
        install ??= await locator.FindDshAsync();
        if (install is null) return;
        IReadOnlyList<GlobalDshProcess> processes = GlobalDshProcessInspector.Find(install);
        if (processes.Count == 0) return;

        if (confirm)
        {
            if (!DialogService.Confirm(
                    "停止 npm 全局版 DSH",
                    $"检测到 {processes.Count} 个 DSH 服务器。是否停止？\r\n不会停止其他 Node.js 程序。",
                    warning: true))
            {
                return;
            }
        }

        foreach (GlobalDshProcess process in processes)
        {
            GlobalDshProcessInspector.StopVerified(process, install);
        }
    }

    private void OnServerStopped(object? sender, EventArgs eventArgs)
    {
        if (!busy && !intentionalStop)
        {
            _ = InvokeOnUiAsync(() =>
            {
                tray.Visible = false;
                ExitThread();
                return Task.CompletedTask;
            });
        }
    }

    private Task InvokeOnUiAsync(Func<Task> action)
    {
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        dispatcher.BeginInvoke(new Action(async () =>
        {
            try
            {
                await action();
                completion.SetResult();
            }
            catch (Exception exception)
            {
                completion.SetException(exception);
            }
        }));
        return completion.Task;
    }

    private void ShowBalloon(string message, ToolTipIcon icon)
    {
        tray.BalloonTipTitle = "DSH Launcher";
        tray.BalloonTipText = message;
        tray.BalloonTipIcon = icon;
        tray.ShowBalloonTip(5000);
    }
}
