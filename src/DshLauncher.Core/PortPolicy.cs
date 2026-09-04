namespace DshLauncher.Core;

public static class PortPolicy
{
    public const int PreferredPort = 3080;
    public const int FallbackStart = 55000;
    public const int FallbackEnd = 64999;

    private static readonly HashSet<int> ExcludedPorts =
    [
        55000, // Reserved as a boundary; selection begins at the following port.
        56789, // Commonly used by local development utilities.
        60000, // Common round-number choice for local services.
        63790, // Avoid visually confusing this with Redis 6379 in support logs.
    ];

    public static bool IsAllowedFallback(int port)
    {
        return port > FallbackStart && port <= FallbackEnd && !ExcludedPorts.Contains(port);
    }
}
