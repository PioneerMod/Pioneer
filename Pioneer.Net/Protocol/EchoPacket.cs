using DotNetty.Buffers;
using Pioneer.Net.Packet;

namespace Pioneer.Net.Protocol;

[PacketId(0x01)]
public class EchoPacket : IPacket
{
    public string Message { get; set; } = "ECHO";
        
    public void Write(IByteBuffer buffer)
    {
        buffer.WriteStringUTF8(Message);
    }

    public void Read(IByteBuffer buffer)
    {
        Message = buffer.ReadStringUTF8();
    }
}