namespace Pioneer.Net.Packet;

/// <summary>
/// Defines the packet ID for the underlying packet class utilizing the <see cref="IPacket"/> interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class PacketId : Attribute
{
    /// <summary>
    /// The id of the packet.
    /// </summary>
    public int Value { get; }

    public PacketId(int value)
    {
        Value = value;
    }
}