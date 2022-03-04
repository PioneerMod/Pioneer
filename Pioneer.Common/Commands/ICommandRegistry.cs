namespace Pioneer.Common.Commands;

/// <summary>
/// The command registry manages all commands (wow what a miracle).
/// </summary>
public interface ICommandRegistry
{
    /// <summary>
    /// Scans through the object and registers all <see cref="Command"/>s. Only non-static methods will be registered.
    /// </summary>
    /// <param name="obj">The object which owns the commands.</param>
    void Scan(object obj);

    /// <summary>
    /// Scans through the class and registers all <see cref="Command"/>s. Only static methods will be registered.
    /// </summary>
    /// <typeparam name="T">The class which owns the commands.</typeparam>
    void Scan<T>();

    /// <summary>
    /// Returns all names of the available commands.
    /// </summary>
    IEnumerable<string> GetCommandNames();

    /// <summary>
    /// Returns all usages of the available commands.
    /// </summary>
    IEnumerable<string> GetCommandUsages();

    /// <summary>
    /// Processes the given line and tries to find and execute a matching <see cref="Command"/> handler.
    /// </summary>
    /// <param name="line">The line to process.</param>
    Task<bool> ProcessAsync(string line);

    /// <summary>
    /// Creates a new registry and returns it (it won't be cached internally).
    /// </summary>
    public static ICommandRegistry Create()
    {
        return new CommandRegistry();
    }
}