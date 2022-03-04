using Pioneer.Net.Packet;

namespace Pioneer.Net.Protocol.Client;

[PacketId(-0x01)]
public class RequestAuthPacket : MappedPacket
{
    [Field]
    public string Password { get; private set; } = "";
    
    public RequestAuthPacket() {}

    public RequestAuthPacket(string password)
    {
        Password = password;
    }
}