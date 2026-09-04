using System.Security.AccessControl;
using System.Security.Principal;

namespace DshLauncher;

internal sealed class AppPaths
{
    public AppPaths()
    {
        Root = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "DshLauncher");
        Logs = Path.Combine(Root, "logs");
        Downloads = Path.Combine(Root, "downloads");
        State = Path.Combine(Root, "state.json");
        UpdateCache = Path.Combine(Root, "update-cache.json");
        TaskState = Path.Combine(Root, "task-state.json");
        Patch = Path.Combine(Root, "launcher.patch.yml");
        Plugin = Path.Combine(Root, "launcher-status-plugin.mjs");

        Directory.CreateDirectory(Root);
        Directory.CreateDirectory(Logs);
        Directory.CreateDirectory(Downloads);
        TryRestrictToCurrentUser(Root);
    }

    public string Root { get; }
    public string Logs { get; }
    public string Downloads { get; }
    public string State { get; }
    public string UpdateCache { get; }
    public string TaskState { get; }
    public string Patch { get; }
    public string Plugin { get; }

    private static void TryRestrictToCurrentUser(string path)
    {
        try
        {
            SecurityIdentifier user = WindowsIdentity.GetCurrent().User
                ?? throw new InvalidOperationException("The current Windows identity has no SID.");
            var security = new DirectorySecurity();
            security.SetAccessRuleProtection(isProtected: true, preserveInheritance: false);
            security.AddAccessRule(new FileSystemAccessRule(
                user,
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            security.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            security.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                FileSystemRights.FullControl,
                InheritanceFlags.ContainerInherit | InheritanceFlags.ObjectInherit,
                PropagationFlags.None,
                AccessControlType.Allow));
            new DirectoryInfo(path).SetAccessControl(security);
        }
        catch
        {
            // A locked-down enterprise policy may reject ACL replacement. The
            // default per-user LocalAppData ACL remains the safe fallback.
        }
    }
}
