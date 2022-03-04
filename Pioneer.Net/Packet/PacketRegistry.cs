using System.Collections.Concurrent;
using System.Reflection;
using System.Text;
using DotNetty.Buffers;
using Pioneer.Net.Protocol;
using Pioneer.Net.Protocol.Client;
using Pioneer.Net.Protocol.Server;

namespace Pioneer.Net.Packet;

/// <summary>
/// The packet registry allows registration of packet types as well as management of these. From here the packets
/// are getting identified and if a packet is being sent or being received the registry will make sure, that
/// the real packet stays as the real packet.
/// </summary>
public static class PacketRegistry
{
    internal static readonly ConcurrentDictionary<Type, PacketMapping> Mappings = new();
    private static readonly Dictionary<int, Type> PacketIdToType = new();
    private static readonly Dictionary<Type, int> TypeToPacketId = new();

    /// <summary>
    /// Initializes the packet registry. Through the initialization the built in packets are getting
    /// registered which will be needed.
    /// </summary>
    public static void Initialize()
    {
        Register<EchoPacket>(); // Test Packet
        Register<KeepAlivePacket>();
        Register<RequestAuthPacket>();
        Register<AuthFinishPacket>();
    }

    /// <summary>
    /// Registers the given type as packet type and connects it with the given id.
    /// </summary>
    /// <typeparam name="T">The type of the packet.</typeparam>
    /// <exception cref="NotSupportedException">If the id is already being used.</exception>
    /// <exception cref="ArgumentException">If the given type has no packet id.</exception>
    public static void Register<T>() where T : IPacket
    {
        var type = typeof(T);
        var packetId = type.GetCustomAttribute<PacketId>();
        if (packetId == null)
            throw new ArgumentException("No PacketId found on " + type.FullName);

        if (PacketIdToType.ContainsKey(packetId.Value))
            throw new NotSupportedException(
                $"Same PacketId used twice: {PacketIdToType[packetId.Value].FullName} and {type.FullName}");

        if (typeof(MappedPacket).IsAssignableFrom(type))
        {
            GenerateMapping(type);
        }

        PacketIdToType.Add(packetId.Value, type);
        TypeToPacketId.Add(type, packetId.Value);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="packet"></param>
    /// <returns></returns>
    internal static IByteBuffer ConvertToByteBuffer(IPacket packet)
    {
        var packetType = packet.GetType();
        if (!TypeToPacketId.ContainsKey(packetType))
            throw new NotSupportedException($"The packet {packetType.FullName} is not registered!");

        var buffer = Unpooled.Buffer();
        buffer.WriteInt(TypeToPacketId[packetType]);
        packet.Write(buffer);
        return buffer;
    }

    internal static IPacket? ConvertFromByteBuffer(IByteBuffer buffer)
    {
        var packetId = buffer.ReadInt();
        if (!PacketIdToType.ContainsKey(packetId))
            throw new ArgumentException($"The packet id {packetId} has no associated packet!");

        if (Activator.CreateInstance(PacketIdToType[packetId]) is not IPacket packet)
            return null;
        packet.Read(buffer);
        return packet;
    }

    public static void WriteStringUTF8(this IByteBuffer buffer, string data)
    {
        var bytes = Encoding.UTF8.GetBytes(data);
        buffer.WriteInt(bytes.Length);
        buffer.WriteBytes(bytes);
    }

    public static string ReadStringUTF8(this IByteBuffer buffer)
    {
        var length = buffer.ReadInt();
        var bytes = new byte[length];
        buffer.ReadBytes(bytes);
        return Encoding.UTF8.GetString(bytes);
    }
    
    private static void GenerateMapping(Type packetType)
    {
        var mapping = PacketMapping.Generate(packetType);
        Mappings.TryAdd(packetType, mapping);
    }
}