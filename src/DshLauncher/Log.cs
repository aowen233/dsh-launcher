using DshLauncher.Core;

namespace DshLauncher;

internal sealed class Log
{
    private readonly object sync = new();
    private readonly string path;

    public Log(AppPaths paths)
    {
        path = Path.Combine(paths.Logs, $"launcher-{DateTime.UtcNow:yyyyMMdd}.log");
    }

    public void Write(string message)
    {
        string safe = SecretRedactor.Redact(message).ReplaceLineEndings(" ");
        lock (sync)
        {
            File.AppendAllText(path, $"{DateTimeOffset.Now:O} {safe}{Environment.NewLine}");
        }
    }
}
