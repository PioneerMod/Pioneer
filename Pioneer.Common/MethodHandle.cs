using System.Reflection;

namespace Pioneer.Common;

/// <summary>
/// The method handle combines a method's owner and its method in one class, which than can be called via the
/// <see cref="Invoke"/> method.
/// </summary>
public class MethodHandle
{
    /// <summary>
    /// The possible owner of the method. If the owner is null, the method is probably a static method.
    /// </summary>
    public object? Owner { get; }
    
    /// <summary>
    /// The method which is getting hold by this handle.
    /// </summary>
    public MethodInfo Method { get; }

    public MethodHandle(MethodInfo method, object? owner = null)
    {
        Owner = owner;
        Method = method;
    }

    /// <summary>
    /// Invokes the given method and returns the return value (if there is one).
    /// </summary>
    /// <param name="args">The arguments which will be passed to the method.</param>
    /// <returns>The return value of the method or null.</returns>
    public object? Invoke(params object?[]? args)
    {
        return Method.Invoke(Owner, args);
    } 
}