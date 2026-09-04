using System.Diagnostics;

namespace DshLauncher;

internal static class BrowserService
{
    public static void Open(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out Uri? uri) ||
            uri.Scheme != Uri.UriSchemeHttp ||
            uri.Host != "127.0.0.1")
        {
            throw new InvalidOperationException("Only the local DSH URL may be opened.");
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = uri.AbsoluteUri,
            UseShellExecute = true,
        });
    }
}
