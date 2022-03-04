using Pioneer.Net;
using Pioneer.Net.Packet;
using Pioneer.Net.Protocol;

namespace Pioneer.Client.Test;

internal class TestClient
{
    public static void Start()
    {
        string ip = "127.0.0.1";
        int port = 2077;
        LoggingInterface.Error += exception =>
        {
            Console.WriteLine(exception.ToString());
        };
        PacketRegistry.Initialize();
        PacketHandler.Scan<TestClient>();
        Network.ConnectAsync(ip, port, "Change Me!", client =>
        {
            Console.WriteLine("Connected!");
            client.SendPacket(new EchoPacket{Message = "Hello World!"});
        }).GetAwaiter().GetResult();
        Console.ReadLine();
    }

    [PacketHandler(typeof(EchoPacket))]
    public static void OnEchoPacket(EchoPacket packet)
    {
        Console.WriteLine("Echo Packet received: " + packet.Message);
    }
}