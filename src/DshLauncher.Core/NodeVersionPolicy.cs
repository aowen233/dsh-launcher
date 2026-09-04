namespace DshLauncher.Core;

public static class NodeVersionPolicy
{
    public const int PreferredMajor = 24;

    public static bool IsSupported(SemanticVersion version)
    {
        return (version.Major == 22 &&
                (version.Minor > 19 || version.Minor == 19 && version.Patch >= 0)) ||
               version.Major >= 24;
    }
}
