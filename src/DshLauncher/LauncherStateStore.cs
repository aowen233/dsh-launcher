using System.Diagnostics;
using System.Text.Json;

namespace DshLauncher;

internal sealed record RecoveredDshSession(int ProcessId, int Port, string AccessUrl);

internal sealed class LauncherStateStore(AppPaths paths, Log log)
{
    public async Task WriteAsync(
        string status,
        Process? process,
        int port,
        DshInstall? install,
        string? accessUrl,
        CancellationToken cancellationToken)
    {
        var state = new
        {
            status,
            serverPid = process?.Id,
            port,
            dshVersion = install?.Version.ToString(),
            dshInstallDirectory = install?.PackageDirectory,
            protectedAccessUrl = accessUrl is null ? null : DpapiSecret.Protect(accessUrl),
            updatedAt = DateTimeOffset.UtcNow,
        };
        string temporary = paths.State + ".tmp";
        await File.WriteAllTextAsync(temporary, JsonSerializer.Serialize(state), cancellationToken);
        File.Move(temporary, paths.State, overwrite: true);
    }

    public RecoveredDshSession? TryRecover(
        DshInstall install,
        IReadOnlyList<GlobalDshProcess> verifiedProcesses)
    {
        try
        {
            if (!File.Exists(paths.State)) return null;
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(paths.State));
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("status", out JsonElement status) ||
                status.GetString() != "ready" ||
                !root.TryGetProperty("serverPid", out JsonElement pidElement) ||
                !root.TryGetProperty("port", out JsonElement portElement) ||
                !root.TryGetProperty("dshInstallDirectory", out JsonElement directoryElement) ||
                !root.TryGetProperty("protectedAccessUrl", out JsonElement urlElement))
            {
                return null;
            }

            int processId = pidElement.GetInt32();
            int port = portElement.GetInt32();
            string? recordedDirectory = directoryElement.GetString();
            string? encryptedUrl = urlElement.GetString();
            if (port is < 1 or > 65535 || string.IsNullOrWhiteSpace(recordedDirectory) ||
                string.IsNullOrWhiteSpace(encryptedUrl) ||
                !Path.GetFullPath(recordedDirectory).Equals(
                    Path.GetFullPath(install.PackageDirectory),
                    StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            GlobalDshProcess? verified = verifiedProcesses.FirstOrDefault(
                item => item.ProcessId == processId && item.Port == port);
            if (verified is null)
            {
                using Process candidate = Process.GetProcessById(processId);
                string? executable = candidate.MainModule?.FileName;
                if (candidate.HasExited || string.IsNullOrWhiteSpace(executable) ||
                    !Path.GetFullPath(executable).Equals(
                        Path.GetFullPath(install.Node.NodePath),
                        StringComparison.OrdinalIgnoreCase))
                {
                    return null;
                }
                log.Write("Using protected launcher state because WMI command-line verification was unavailable.");
            }

            string accessUrl = DpapiSecret.Unprotect(encryptedUrl);
            if (!Uri.TryCreate(accessUrl, UriKind.Absolute, out Uri? uri) ||
                uri.Scheme != Uri.UriSchemeHttp ||
                uri.Host != "127.0.0.1" ||
                uri.Port != port)
            {
                return null;
            }

            return new RecoveredDshSession(processId, port, uri.AbsoluteUri);
        }
        catch (Exception exception)
        {
            log.Write($"Ignoring launcher state that could not be safely recovered: {exception.Message}");
            return null;
        }
    }
}
