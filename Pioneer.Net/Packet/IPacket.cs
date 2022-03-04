using DotNetty.Buffers;

namespace Pioneer.Net.Packet;

/// <summary>
/// The packet is a container for data which gets transported over the network from client to server
/// or vice versa. Packets have two methods which will either load packet data from a <see cref="IByteBuffer"/>
/// or writes data to the packet buffer.
/// </summary>
public interface IPacket
{
    /// <summary>
    /// Gets called when the packet should be sent and the data needs to get written to the given buffer.
    /// </summary>
    /// <param name="buffer">The buffer which will store the data over the network.</param>
    void Write(IByteBuffer buffer);

    /// <summary>
    /// Gets called before the packet gets passed to the packet handler after reception. The data
    /// can be read from the buffer and stored into this packet instance.
    /// </summary>
    /// <param name="buffer">The buffer which stored the data over the network.</param>
    void Read(IByteBuffer buffer);
}