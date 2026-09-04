using System.Text.RegularExpressions;

namespace DshLauncher.Core;

public static partial class SecretRedactor
{
    public static string Redact(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        string redacted = BearerRegex().Replace(value, "$1[REDACTED]");
        redacted = QuerySecretRegex().Replace(redacted, "$1[REDACTED]");
        redacted = EnvironmentSecretRegex().Replace(redacted, "$1[REDACTED]");
        return redacted;
    }

    [GeneratedRegex("(?i)(authorization\\s*:\\s*bearer\\s+)[^\\s,;]+")]
    private static partial Regex BearerRegex();

    [GeneratedRegex("(?i)([?&](?:token|access_token|api_key|key)=)[^&#\\s]+")]
    private static partial Regex QuerySecretRegex();

    [GeneratedRegex("(?i)((?:DEEPSEEK_API_KEY|DEEPSEEK_PLATFORM_TOKEN|DSH_TOKEN)\\s*=\\s*)[^\\s]+")]
    private static partial Regex EnvironmentSecretRegex();
}
