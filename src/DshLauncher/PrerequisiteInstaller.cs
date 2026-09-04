using System.ComponentModel;
using System.Diagnostics;

namespace DshLauncher;

internal sealed class PrerequisiteInstaller(
    DependencyManifest manifest,
    VerifiedDownloader downloader,
    AppPaths paths,
    Log log)
{
    public async Task InstallNodeAsync(CancellationToken cancellationToken = default)
    {
        DownloadDependency dependency = manifest.CurrentNodeInstaller();
        log.Write($"Downloading approved Node.js {manifest.Node.Version} installer from nodejs.org.");
        string msi = await downloader.DownloadAsync(dependency, paths.Downloads, cancellationToken: cancellationToken);

        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "msiexec.exe"),
            UseShellExecute = true,
            Verb = "runas",
        };
        startInfo.ArgumentList.Add("/i");
        startInfo.ArgumentList.Add(msi);
        startInfo.ArgumentList.Add("/passive");
        startInfo.ArgumentList.Add("/norestart");

        try
        {
            using Process process = Process.Start(startInfo)
                ?? throw new InvalidOperationException("Could not start the Node.js installer.");
            await process.WaitForExitAsync(cancellationToken);
            if (process.ExitCode is not 0 and not 3010)
            {
                throw new InvalidOperationException($"Node.js installer exited with code {process.ExitCode}.");
            }
            log.Write($"Node.js installer completed with code {process.ExitCode}.");
        }
        catch (Win32Exception exception) when (exception.NativeErrorCode == 1223)
        {
            throw new OperationCanceledException("Node.js installation was cancelled by the user.", exception);
        }
    }
}
