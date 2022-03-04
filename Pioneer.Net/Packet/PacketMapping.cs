using System.Reflection;
using DotNetty.Buffers;
using SpanJson;
using static Pioneer.Common.Commands.Converters.BasicConverter;

namespace Pioneer.Net.Packet;

/// <summary>
/// The packet mapping allows automatic serialization and deserialization of classes which extend the <see cref="Packet"/>
/// class.
/// </summary>
internal class PacketMapping
{
    private readonly LinkedList<Action<IByteBuffer, object>> _serialization, _deserialization;

    private PacketMapping()
    {
        _serialization = new LinkedList<Action<IByteBuffer, object>>();
        _deserialization = new LinkedList<Action<IByteBuffer, object>>();
    }

    internal void Serialize(IByteBuffer buffer, object obj)
    {
        foreach (var serialize in _serialization)
        {
            serialize(buffer, obj);
        }
    }

    internal void Deserialize(IByteBuffer buffer, object obj)
    {
        foreach (var deserialize in _deserialization)
        {
            deserialize(buffer, obj);
        }
    }

    internal static PacketMapping Generate(Type type)
    {
        var mapping = new PacketMapping();
        foreach (var field in type.GetRuntimeProperties().Where(field => field.GetCustomAttribute<FieldAttribute>() != null)
                     .OrderBy(field => field.GetCustomAttribute<FieldAttribute>()?.Index ?? 0))
        {
            var shouldSerialize = field.GetCustomAttribute<FieldAttribute>()?.Serialize ?? false;
            mapping._serialization.AddLast((buffer, obj) =>
            {
                var val = GetObjectValue(field, obj);
                if (val != null)
                {
                    if (shouldSerialize)
                    {
                        var data = JsonSerializer.NonGeneric.Utf8.Serialize(val);
                        buffer.WriteInt(data.Length);
                        buffer.WriteBytes(data);
                    }
                    else
                    {
                        WriteBufferForType(buffer, field.PropertyType, val);
                    }
                }
            });
            mapping._deserialization.AddLast((buffer, obj) =>
            {
                object? val;
                if (shouldSerialize)
                {
                    var length = buffer.ReadInt();
                    var bytes = new byte[length];
                    buffer.ReadBytes(bytes);
                    val = JsonSerializer.NonGeneric.Utf8.Deserialize(bytes, field.PropertyType);
                }
                else
                {
                    val = ReadBufferForType(buffer, field.PropertyType);
                }
                if (val != null)
                {
                    SetObjectValue(field, obj, val);
                }
            });
        }

        return mapping;
    }

    private static object? GetObjectValue(PropertyInfo field, object obj)
    {
        return field.GetValue(obj);
    }

    private static void SetObjectValue(PropertyInfo field, object obj, object val)
    {
        field.SetValue(obj, val);
    }

    private static void WriteBufferForType(IByteBuffer buffer, Type type, object raw)
    {
        object val = type.IsEnum ? Convert.ToInt32(raw) : raw;
        switch (val)
        {
            case ushort u:
                buffer.WriteUnsignedShort(u);
                break;
            case int i:
                buffer.WriteInt(i);
                break;
            case long l:
                buffer.WriteLong(l);
                break;
            case short s:
                buffer.WriteShort(s);
                break;
            case byte b:
                buffer.WriteByte(b);
                break;
            case char c:
                buffer.WriteChar(c);
                break;
            case float f:
                buffer.WriteFloat(f);
                break;
            case double d:
                buffer.WriteDouble(d);
                break;
            case bool b:
                buffer.WriteBoolean(b);
                break;
            case string s:
                buffer.WriteStringUTF8(s);
                break;
        }
    }

    private static object? ReadBufferForType(IByteBuffer buffer, Type type)
    {
        if (type.IsEnum)
        {
            return Enum.Parse(type, buffer.ReadInt().ToString());
        }
        
        if (type == UShortType)
        {
            return buffer.ReadUnsignedShort();
        }
        
        if (type == IntType)
        {
            return buffer.ReadInt();
        }
        
        if (type == LongType)
        {
            return buffer.ReadLong();
        }
        
        if (type == ShortType)
        {
            return buffer.ReadShort();
        }
        
        if (type == ByteType)
        {
            return buffer.ReadByte();
        }
        
        if (type == CharType)
        {
            return buffer.ReadChar();
        }
        
        if (type == FloatType)
        {
            return buffer.ReadFloat();
        }
        
        if (type == DoubleType)
        {
            return buffer.ReadDouble();
        }
        
        if (type == BoolType)
        {
            return buffer.ReadBoolean();
        }
        
        return type == StringType ? buffer.ReadStringUTF8() : null;
    }
}