using Pioneer.Net.Packet;

namespace Pioneer.Net.Protocol.Server;

[PacketId(-0x02)]
public class AuthFinishPacket : MappedPacket
{
    public long ID { get; private set; }
    
    public AuthFinishPacket() {}

    public AuthFinishPacket(long id)
    {
        ID = id;
    }
}