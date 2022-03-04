namespace Pioneer.Net.Packet;

/// <summary>
/// The field property marks the underlying member as packet field which needs to be serialized or deserialized
/// for network communications.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class FieldAttribute : Attribute
{
    /// <summary>
    /// The index of the field.
    /// </summary>
    public int Index { get; }
    
    /// <summary>
    /// Whether this field will be serialized or not.
    /// </summary>
    public bool Serialize { get; set; }

    public FieldAttribute(int index = 0)
    {
        Index = index;
    }
}