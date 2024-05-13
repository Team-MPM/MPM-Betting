using System.Net.Sockets;

namespace MPM_Betting.IntegrationTests;

public class TcpConnectionTester
{
    public static bool TestConnection(string server, int port)
    {
        using var client = new TcpClient();
        try
        {
            client.Connect(server, port);
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }
}