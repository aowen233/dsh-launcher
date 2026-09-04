using System.Text.Json;

namespace DshLauncher;

internal static class TaskStateReader
{
    public static int ReadRunningCount(string path, int expectedServerPid)
    {
        try
        {
            if (!File.Exists(path)) return -1;
            using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
            JsonElement root = document.RootElement;
            if (!root.TryGetProperty("serverPid", out JsonElement pid) ||
                pid.GetInt32() != expectedServerPid ||
                !root.TryGetProperty("running", out JsonElement running) ||
                !root.TryGetProperty("updatedAt", out JsonElement updatedAt) ||
                DateTimeOffset.UtcNow - updatedAt.GetDateTimeOffset() > TimeSpan.FromSeconds(5))
            {
                return -1;
            }
            return running.GetInt32();
        }
        catch
        {
            return -1;
        }
    }
}
