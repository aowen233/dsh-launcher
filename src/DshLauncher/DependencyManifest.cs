using System.Runtime.InteropServices;
using System.Text.Json;

namespace DshLauncher;

internal sealed class DependencyManifest
{
    public required NodeDependency Node { get; init; }
    public required DshDependency Dsh { get; init; }
    public required WhaleDependency WhaleWidget { get; init; }

    public static DependencyManifest Load()
    {
        using Stream stream = typeof(DependencyManifest).Assembly.GetManifestResourceStream(
            "DshLauncher.Resources.dependencies.json")
            ?? throw new InvalidDataException("Embedded dependency manifest is missing.");
        using var reader = new StreamReader(stream);
        string json = reader.ReadToEnd();
        return JsonSerializer.Deserialize<DependencyManifest>(json, JsonOptions())
            ?? throw new InvalidDataException("Dependency manifest is empty.");
    }

    public DownloadDependency CurrentNodeInstaller()
    {
        return RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => Node.X64,
            Architecture.Arm64 => Node.Arm64,
            _ => throw new PlatformNotSupportedException("Only Windows x64 and arm64 are supported."),
        };
    }

    private static JsonSerializerOptions JsonOptions() => new()
    {
        PropertyNameCaseInsensitive = true,
    };
}

internal sealed class NodeDependency
{
    public required string Version { get; init; }
    public required DownloadDependency X64 { get; init; }
    public required DownloadDependency Arm64 { get; init; }
}

internal sealed class DownloadDependency
{
    public required string Url { get; init; }
    public required string Sha256 { get; init; }
}

internal sealed class DshDependency
{
    public required string Package { get; init; }
    public required string InstallSpec { get; init; }
}

internal sealed class WhaleDependency
{
    public bool Enabled { get; init; }
    public required string Name { get; init; }
    public required string Version { get; init; }
    public required string Author { get; init; }
    public required string Repository { get; init; }
    public required string Commit { get; init; }
    public required string DshInstallSpec { get; init; }
}
