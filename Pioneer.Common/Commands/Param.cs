using System.Reflection;

namespace Pioneer.Common.Commands;

/// <summary>
/// Defines specific meta information for the underlying parameter, which are used in the command framework.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
public class Param : Attribute
{
    /// <summary>
    /// The name of the parameter.
    /// </summary>
    public string? Name { get; }
    
    /// <summary>
    /// The description of the parameter what its used for.
    /// </summary>
    public string Description { get; set; }
    
    /// <summary>
    /// Whether this parameter is greedy (if it consumes all of the parameters or not).
    /// </summary>
    public bool IsGreedy { get; set; }

    public Param(string? name = null, string description = "No description")
    {
        Name = name;
        Description = description;
    }
}