using DotNetty.Buffers;
using Pioneer.Net.Packet;

namespace Pioneer.Net.Protocol.Server;

[PacketId(-0x02)]
public class AuthFinishPacket : IPacket
{
    public long ID { get; private set; }
    
    public AuthFinishPacket() {}

    public AuthFinishPacket(long id)
    {
        ID = id;
    }
    
    public void Write(IByteBuffer buffer)
    {
        buffer.WriteLong(ID);
    }

    public void Read(IByteBuffer buffer)
    {
        ID = buffer.ReadLong();
    }
}