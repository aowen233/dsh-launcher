using System.Security.Cryptography;

namespace DshLauncher;

internal sealed class VerifiedDownloader
{
    private static readonly HashSet<string> AllowedHosts = new(StringComparer.OrdinalIgnoreCase)
    {
        "nodejs.org",
        "registry.npmjs.org",
        "github.com",
        "codeload.github.com",
    };

    private readonly HttpClient client = new()
    {
        Timeout = TimeSpan.FromMinutes(10),
    };

    public VerifiedDownloader()
    {
        client.DefaultRequestHeaders.UserAgent.ParseAdd("DSH-Launcher/0.1");
    }

    public async Task<string> DownloadAsync(
        DownloadDependency dependency,
        string destinationDirectory,
        IProgress<int>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var uri = new Uri(dependency.Url, UriKind.Absolute);
        if (uri.Scheme != Uri.UriSchemeHttps || !AllowedHosts.Contains(uri.Host))
        {
            throw new InvalidOperationException($"Download origin is not allowed: {uri.Host}");
        }

        Directory.CreateDirectory(destinationDirectory);
        string fileName = Path.GetFileName(uri.LocalPath);
        string partialPath = Path.Combine(destinationDirectory, $"{fileName}.{Guid.NewGuid():N}.partial");
        string finalPath = Path.Combine(destinationDirectory, fileName);

        try
        {
            using HttpResponseMessage response = await client.GetAsync(
                uri,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            long? total = response.Content.Headers.ContentLength;
            await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var destination = new FileStream(
                partialPath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                1024 * 128,
                FileOptions.Asynchronous | FileOptions.SequentialScan);

            var buffer = new byte[1024 * 128];
            long written = 0;
            while (true)
            {
                int read = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
                if (read == 0) break;
                await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken).ConfigureAwait(false);
                written += read;
                if (total > 0) progress?.Report((int)Math.Min(100, written * 100 / total.Value));
            }

            await destination.FlushAsync(cancellationToken).ConfigureAwait(false);
            destination.Close();

            await using var verificationStream = File.OpenRead(partialPath);
            string actual = Convert.ToHexString(await SHA256.HashDataAsync(
                verificationStream,
                cancellationToken)).ToLowerInvariant();
            if (!CryptographicOperations.FixedTimeEquals(
                    Convert.FromHexString(actual),
                    Convert.FromHexString(dependency.Sha256)))
            {
                throw new InvalidDataException("Downloaded file failed SHA-256 verification.");
            }

            File.Move(partialPath, finalPath, overwrite: true);
            return finalPath;
        }
        finally
        {
            if (File.Exists(partialPath)) File.Delete(partialPath);
        }
    }
}
