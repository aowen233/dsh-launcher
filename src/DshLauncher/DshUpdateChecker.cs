using System.Text.Json;
using DshLauncher.Core;

namespace DshLauncher;

internal sealed class DshUpdateChecker(NpmManager npm, AppPaths paths, Log log)
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromHours(6);

    public async Task<SemanticVersion?> GetLatestAsync(
        NodeInstall node,
        bool forceRefresh = false,
        CancellationToken cancellationToken = default)
    {
        if (!forceRefresh && TryReadCache(out SemanticVersion? cached)) return cached;

        SemanticVersion? latest = await npm.QueryLatestDshVersionAsync(node, cancellationToken);
        if (latest is not null) await SaveAsync(latest, cancellationToken);
        return latest;
    }

    public Task RecordInstalledAsync(SemanticVersion version, CancellationToken cancellationToken = default)
    {
        return SaveAsync(version, cancellationToken);
    }

    private bool TryReadCache(out SemanticVersion? version)
    {
        version = null;
        try
        {
            if (!File.Exists(paths.UpdateCache)) return false;
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(paths.UpdateCache));
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("checkedAt", out JsonElement checkedAt) ||
                !root.TryGetProperty("version", out JsonElement versionElement))
            {
                return false;
            }

            TimeSpan age = DateTimeOffset.UtcNow - checkedAt.GetDateTimeOffset();
            if (age < TimeSpan.Zero || age >= CacheLifetime ||
                !SemanticVersion.TryParse(versionElement.GetString(), out version))
            {
                return false;
            }
            return true;
        }
        catch (Exception exception)
        {
            log.Write($"Ignoring invalid update cache: {exception.Message}");
            return false;
        }
    }

    private async Task SaveAsync(SemanticVersion version, CancellationToken cancellationToken)
    {
        string temporary = paths.UpdateCache + ".tmp";
        string json = JsonSerializer.Serialize(new
        {
            version = version.ToString(),
            checkedAt = DateTimeOffset.UtcNow,
        });
        await File.WriteAllTextAsync(temporary, json, cancellationToken);
        File.Move(temporary, paths.UpdateCache, overwrite: true);
    }
}
