namespace DshLauncher;

internal static class Program
{
    private const string MutexName = "Local\\DshLauncher.PrimaryInstance.v1";

    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();

        using var mutex = new Mutex(true, MutexName, out bool isPrimary);
        if (!isPrimary)
        {
            SingleInstanceClient.Send(args.Length == 0 ? "open" : string.Join(' ', args));
            return;
        }

        using var context = new LauncherApplicationContext(args);
        Application.Run(context);
    }
}
