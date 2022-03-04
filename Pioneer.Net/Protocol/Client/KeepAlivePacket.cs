using DotNetty.Buffers;
using Pioneer.Net.Packet;

namespace Pioneer.Net.Protocol.Client;

/// <summary>
/// The keep alive packet will be sent every minute to keep the connection between the server and the client alive.
/// </summary>
[PacketId(0x0)]
public class KeepAlivePacket : IPacket
{
    public void Write(IByteBuffer buffer)
    {
    }

    public void Read(IByteBuffer buffer)
    {
    }
}