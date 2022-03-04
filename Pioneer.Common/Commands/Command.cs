namespace Pioneer.Common.Commands;

/// <summary>
/// Defines the class or method as command holder. If the underlying member is a class, every command in that class
/// will be a children of this as class defined command. When using class defined commands, marking a method without
/// name, this method will be called when the base command is called.
/// If the underlying member is a method, it just defines the handler for this command.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class Command : Attribute
{
    /// <summary>
    /// Defines this command as index command.
    /// </summary>
    public const string Index = "[INDEX]";
    
    /// <summary>
    /// The name of the command through which this command gets called.
    /// </summary>
    public string Name { get; }
    
    /// <summary>
    /// The description of the command what the command will do.
    /// </summary>
    public string Description { get; set; }

    public Command(string name, string description = "No description")
    {
        Name = name;
        Description = description;
    }

    public Command() : this(Index)
    {
    }
}