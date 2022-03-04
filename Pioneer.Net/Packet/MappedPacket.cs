using DotNetty.Buffers;

namespace Pioneer.Net.Packet;

/// <summary>
/// The mapped packet is a special packet designed for simple serialization and deserialization of packets
/// without writing their respective <see cref="IPacket.Write"/> and <see cref="IPacket.Read"/> methods. Therefore
/// a <see cref="PacketMapping"/> will be generated which than will be used for the serialization.
/// </summary>
public abstract class MappedPacket : IPacket
{
    private readonly PacketMapping? _mapping; 

    protected MappedPacket()
    {
        var type = GetType();
        if (PacketRegistry.Mappings.ContainsKey(type))
            _mapping = PacketRegistry.Mappings[type];
    }

    public void Write(IByteBuffer buffer)
    {
        _mapping?.Serialize(buffer, this);
    }

    public void Read(IByteBuffer buffer)
    {
        _mapping?.Deserialize(buffer, this);
    }
}