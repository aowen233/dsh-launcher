using System.Diagnostics;
using System.Management;
using System.Text.RegularExpressions;
using DshLauncher.Core;

namespace DshLauncher;

internal sealed record GlobalDshProcess(int ProcessId, int Port, string CommandLine);

internal static partial class GlobalDshProcessInspector
{
    public static IReadOnlyList<GlobalDshProcess> Find(DshInstall install)
    {
        var matches = new List<GlobalDshProcess>();
        try
        {
            using var searcher = new ManagementObjectSearcher(
                @"root\cimv2",
                "SELECT ProcessId, CommandLine FROM Win32_Process WHERE Name='node.exe'");
            using ManagementObjectCollection results = searcher.Get();
            foreach (ManagementObject item in results)
            {
                using (item)
                {
                    string? commandLine = item["CommandLine"] as string;
                    int processId = Convert.ToInt32(
                        item["ProcessId"],
                        System.Globalization.CultureInfo.InvariantCulture);
                    if (string.IsNullOrWhiteSpace(commandLine) || processId == Environment.ProcessId) continue;
                    if (!commandLine.Contains(install.EntryPoint, StringComparison.OrdinalIgnoreCase)) continue;
                    if (!WebCommandRegex().IsMatch(commandLine)) continue;

                    Match portMatch = PortRegex().Match(commandLine);
                    int port = portMatch.Success && int.TryParse(portMatch.Groups[1].Value, out int parsed)
                        ? parsed
                        : PortPolicy.PreferredPort;
                    matches.Add(new GlobalDshProcess(processId, port, SecretRedactor.Redact(commandLine)));
                }
            }
        }
        catch
        {
            // Failure to prove process identity is a safe reason not to touch it.
        }
        return matches;
    }

    public static void StopVerified(GlobalDshProcess candidate, DshInstall install)
    {
        GlobalDshProcess? verified = Find(install).FirstOrDefault(item => item.ProcessId == candidate.ProcessId);
        if (verified is null)
        {
            throw new InvalidOperationException("The DSH process changed before it could be stopped.");
        }

        using Process process = Process.GetProcessById(candidate.ProcessId);
        process.Kill(entireProcessTree: true);
        if (!process.WaitForExit(8000))
        {
            throw new InvalidOperationException($"DSH process {candidate.ProcessId} did not stop.");
        }
    }

    [GeneratedRegex(@"(?:^|\s)web(?:\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex WebCommandRegex();

    [GeneratedRegex(@"--port(?:=|\s+)(\d{1,5})", RegexOptions.IgnoreCase)]
    private static partial Regex PortRegex();
}
