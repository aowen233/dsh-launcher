using System.IO.Pipes;
using System.Text;

namespace DshLauncher;

internal static class SingleInstanceClient
{
    public const string PipeName = "DshLauncher.Control.v1";

    public static void Send(string command)
    {
        try
        {
            using var client = new NamedPipeClientStream(
                ".",
                PipeName,
                PipeDirection.Out,
                PipeOptions.CurrentUserOnly);
            client.Connect(2000);
            using var writer = new StreamWriter(client, new UTF8Encoding(false)) { AutoFlush = true };
            writer.WriteLine(command);
        }
        catch
        {
            DialogService.Information("启动器正在准备", "另一个 DSH Launcher 正在启动，请稍后再试。");
        }
    }
}
