using DotNetty.Buffers;
using Pioneer.Net.Packet;

namespace Pioneer.Net.Protocol.Client;

[PacketId(-0x01)]
public class RequestAuthPacket : IPacket
{
    public string Password { get; private set; } = "";
    
    public RequestAuthPacket() {}

    public RequestAuthPacket(string password)
    {
        Password = password;
    }
    
    public void Write(IByteBuffer buffer)
    {
        buffer.WriteStringUTF8(Password);
    }

    public void Read(IByteBuffer buffer)
    {
        Password = buffer.ReadStringUTF8();
    }
}