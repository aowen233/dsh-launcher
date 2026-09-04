using DshLauncher.Core;

namespace DshLauncher;

internal sealed class NpmManager(DependencyManifest manifest, GlobalDshLocator locator, Log log)
{
    private static readonly string[] AllowedInstallScripts =
    [
        "@deepseek-ai/dsh-subprocess-local",
        "koffi",
        "node-pty",
        "@google/genai",
        "protobufjs",
    ];

    public async Task InstallOrUpdateDshAsync(NodeInstall node, CancellationToken cancellationToken = default)
    {
        string allowScripts = $"--allow-scripts={string.Join(',', AllowedInstallScripts)}";
        ProcessResult result = await ProcessRunner.RunAsync(
            node.NodePath,
            [node.NpmCliPath, "install", "--global", manifest.Dsh.InstallSpec, allowScripts, "--no-audit"],
            TimeSpan.FromMinutes(15),
            cancellationToken);
        log.Write(result.StandardOutput);
        log.Write(result.StandardError);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("npm could not install the global DSH package. See the redacted log.");
        }
        locator.Invalidate();
    }

    public async Task<SemanticVersion?> QueryLatestDshVersionAsync(
        NodeInstall node,
        CancellationToken cancellationToken = default)
    {
        ProcessResult result = await ProcessRunner.RunAsync(
            node.NodePath,
            [node.NpmCliPath, "view", manifest.Dsh.Package, "version", "--json"],
            TimeSpan.FromSeconds(20),
            cancellationToken);
        return result.ExitCode == 0 && SemanticVersion.TryParse(result.StandardOutput, out SemanticVersion? version)
            ? version
            : null;
    }

    public async Task InstallWhaleWidgetAsync(DshInstall dsh, CancellationToken cancellationToken = default)
    {
        if (!manifest.WhaleWidget.Enabled) throw new InvalidOperationException("Whale Widget integration is disabled.");
        ProcessResult result = await ProcessRunner.RunAsync(
            dsh.Node.NodePath,
            [dsh.EntryPoint, "plugin", "--profile", "web", "add", "--workspace-root", manifest.WhaleWidget.DshInstallSpec],
            TimeSpan.FromMinutes(10),
            cancellationToken);
        log.Write(result.StandardOutput);
        log.Write(result.StandardError);
        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException("DSH could not install the Whale Widget. See the redacted log.");
        }
    }
}
