using System.IO.Pipes;
using System.Text;

namespace DshLauncher;

internal sealed class SingleInstanceServer : IDisposable
{
    private readonly CancellationTokenSource cancellation = new();
    private readonly Func<string, Task> handler;
    private readonly Task loop;
    private int disposed;

    public SingleInstanceServer(Func<string, Task> handler)
    {
        this.handler = handler;
        loop = ListenAsync(cancellation.Token);
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref disposed, 1) != 0) return;

        cancellation.Cancel();
        _ = loop.ContinueWith(
            completed =>
            {
                _ = completed.Exception;
                cancellation.Dispose();
            },
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default);
    }

    private async Task ListenAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            await using var pipe = new NamedPipeServerStream(
                SingleInstanceClient.PipeName,
                PipeDirection.In,
                1,
                PipeTransmissionMode.Byte,
                PipeOptions.Asynchronous | PipeOptions.CurrentUserOnly);
            await pipe.WaitForConnectionAsync(cancellationToken);
            using var reader = new StreamReader(pipe, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
            string? command = await reader.ReadLineAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(command)) _ = DispatchAsync(command);
        }
    }

    private async Task DispatchAsync(string command)
    {
        try
        {
            await handler(command);
        }
        catch (ObjectDisposedException) when (Volatile.Read(ref disposed) != 0)
        {
        }
        catch (InvalidOperationException) when (Volatile.Read(ref disposed) != 0)
        {
        }
    }
}
