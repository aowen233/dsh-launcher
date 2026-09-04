using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DshLauncher.Core;

namespace DshLauncher;

internal static class PortAllocator
{
    public static bool IsFree(int port)
    {
        try
        {
            using var listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            listener.Stop();
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    public static int Select()
    {
        if (IsFree(PortPolicy.PreferredPort)) return PortPolicy.PreferredPort;

        int start = Random.Shared.Next(PortPolicy.FallbackStart + 1, PortPolicy.FallbackEnd + 1);
        int count = PortPolicy.FallbackEnd - PortPolicy.FallbackStart;
        for (int offset = 0; offset < count; offset++)
        {
            int candidate = PortPolicy.FallbackStart + 1 +
                ((start - PortPolicy.FallbackStart - 1 + offset) % count);
            if (PortPolicy.IsAllowedFallback(candidate) && IsFree(candidate)) return candidate;
        }

        throw new InvalidOperationException("No safe loopback port is available.");
    }

    public static int CountLoopbackClients(int port)
    {
        return IPGlobalProperties.GetIPGlobalProperties()
            .GetActiveTcpConnections()
            .Count(connection =>
                connection.State == TcpState.Established &&
                connection.LocalEndPoint.Port == port &&
                IPAddress.IsLoopback(connection.RemoteEndPoint.Address));
    }
}
