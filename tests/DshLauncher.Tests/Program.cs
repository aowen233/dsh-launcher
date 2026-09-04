using DshLauncher.Core;

var failures = new List<string>();

Check(SemanticVersion.TryParse("0.1.2-rc.1", out var rc), "parse prerelease");
Check(SemanticVersion.TryParse("0.1.2", out var stable), "parse stable");
Check(stable!.CompareTo(rc) > 0, "stable is newer than prerelease");
Check(SemanticVersion.TryParse("v24.20.0", out var node24), "parse leading v");
Check(NodeVersionPolicy.IsSupported(node24!), "Node 24 is supported");
Check(SemanticVersion.TryParse("22.18.0", out var nodeOld), "parse Node 22");
Check(!NodeVersionPolicy.IsSupported(nodeOld!), "Node 22.18 is rejected");
Check(PortPolicy.IsAllowedFallback(55001), "fallback range starts above boundary");
Check(!PortPolicy.IsAllowedFallback(60000), "excluded common high port");

string redacted = SecretRedactor.Redact(
    "http://127.0.0.1:3080/?token=abc Authorization: Bearer xyz DEEPSEEK_API_KEY=secret");
Check(!redacted.Contains("abc", StringComparison.Ordinal), "query token redacted");
Check(!redacted.Contains("xyz", StringComparison.Ordinal), "bearer token redacted");
Check(!redacted.Contains("secret", StringComparison.Ordinal), "environment secret redacted");

if (failures.Count > 0)
{
    Console.Error.WriteLine(string.Join(Environment.NewLine, failures));
    return 1;
}

Console.WriteLine("All core checks passed.");
return 0;

void Check(bool condition, string name)
{
    if (!condition) failures.Add($"FAILED: {name}");
}
