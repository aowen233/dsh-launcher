using System.Diagnostics;
using System.Text.Json;
using DshLauncher.Core;
using Microsoft.Win32;

namespace DshLauncher;

internal sealed record NodeInstall(string NodePath, string NpmCliPath, SemanticVersion Version);

internal sealed record DshInstall(
    NodeInstall Node,
    string GlobalRoot,
    string PackageDirectory,
    string EntryPoint,
    SemanticVersion Version);

internal sealed class GlobalDshLocator
{
    private NodeInstall? cachedNode;
    private string? cachedGlobalRoot;

    public async Task<NodeInstall?> FindNodeAsync(CancellationToken cancellationToken = default)
    {
        if (cachedNode is not null) return cachedNode;

        foreach (string candidate in EnumerateNodeCandidates().Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(candidate)) continue;
            string npmCli = Path.Combine(Path.GetDirectoryName(candidate)!, "node_modules", "npm", "bin", "npm-cli.js");
            if (!File.Exists(npmCli)) continue;

            ProcessResult result;
            try
            {
                result = await ProcessRunner.RunAsync(candidate, ["--version"], TimeSpan.FromSeconds(5), cancellationToken);
            }
            catch
            {
                continue;
            }

            if (result.ExitCode == 0 && SemanticVersion.TryParse(result.StandardOutput, out SemanticVersion? version))
            {
                cachedNode = new NodeInstall(candidate, npmCli, version!);
                return cachedNode;
            }
        }

        return null;
    }

    public async Task<DshInstall?> FindDshAsync(CancellationToken cancellationToken = default)
    {
        NodeInstall? node = await FindNodeAsync(cancellationToken).ConfigureAwait(false);
        if (node is null) return null;

        cachedGlobalRoot ??= await QueryGlobalRootAsync(node, cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(cachedGlobalRoot)) return null;

        string packageDirectory = Path.Combine(cachedGlobalRoot, "@deepseek-ai", "dsh");
        string manifestPath = Path.Combine(packageDirectory, "package.json");
        if (!File.Exists(manifestPath)) return null;

        using JsonDocument manifest = JsonDocument.Parse(await File.ReadAllTextAsync(manifestPath, cancellationToken));
        JsonElement root = manifest.RootElement;
        if (!root.TryGetProperty("name", out JsonElement name) ||
            name.GetString() != "@deepseek-ai/dsh" ||
            !root.TryGetProperty("version", out JsonElement versionElement) ||
            !SemanticVersion.TryParse(versionElement.GetString(), out SemanticVersion? version))
        {
            return null;
        }

        string relativeEntry = "lib/bin.js";
        if (root.TryGetProperty("bin", out JsonElement bin))
        {
            if (bin.ValueKind == JsonValueKind.String)
            {
                relativeEntry = bin.GetString() ?? relativeEntry;
            }
            else if (bin.ValueKind == JsonValueKind.Object &&
                     bin.TryGetProperty("dsh", out JsonElement dshEntry))
            {
                relativeEntry = dshEntry.GetString() ?? relativeEntry;
            }
        }

        string entry = Path.GetFullPath(Path.Combine(packageDirectory, relativeEntry.Replace('/', Path.DirectorySeparatorChar)));
        if (!entry.StartsWith(packageDirectory + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) ||
            !File.Exists(entry))
        {
            return null;
        }

        return new DshInstall(node, cachedGlobalRoot, packageDirectory, entry, version!);
    }

    public void Invalidate()
    {
        cachedNode = null;
        cachedGlobalRoot = null;
    }

    private static async Task<string?> QueryGlobalRootAsync(NodeInstall node, CancellationToken cancellationToken)
    {
        ProcessResult result = await ProcessRunner.RunAsync(
            node.NodePath,
            [node.NpmCliPath, "root", "--global"],
            TimeSpan.FromSeconds(15),
            cancellationToken);
        return result.ExitCode == 0 ? result.StandardOutput.Trim() : null;
    }

    private static IEnumerable<string> EnumerateNodeCandidates()
    {
        foreach ((RegistryHive hive, string view, string subkey) in new[]
        {
            (RegistryHive.LocalMachine, "64", @"SOFTWARE\Node.js"),
            (RegistryHive.CurrentUser, "64", @"SOFTWARE\Node.js"),
            (RegistryHive.LocalMachine, "32", @"SOFTWARE\Node.js"),
        })
        {
            string? candidate = null;
            try
            {
                RegistryView registryView = view == "64" ? RegistryView.Registry64 : RegistryView.Registry32;
                using RegistryKey baseKey = RegistryKey.OpenBaseKey(hive, registryView);
                using RegistryKey? key = baseKey.OpenSubKey(subkey);
                if (key?.GetValue("InstallPath") is string directory)
                {
                    candidate = Path.Combine(directory, "node.exe");
                }
            }
            catch (UnauthorizedAccessException) { }
            catch (System.Security.SecurityException) { }
            if (candidate is not null) yield return candidate;
        }

        string? path = Environment.GetEnvironmentVariable("PATH");
        if (path is not null)
        {
            foreach (string directory in path.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                string clean = directory.Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(clean)) yield return Path.Combine(clean, "node.exe");
            }
        }

        yield return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "nodejs", "node.exe");
    }
}
