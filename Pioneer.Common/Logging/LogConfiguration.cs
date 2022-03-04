namespace Pioneer.Common.Logging;

/// <summary>
/// The log sink allows specific configurations of the logger where the log message is being put to.
/// </summary>
[Flags]
public enum LogConfiguration
{
    /// <summary>
    /// Prints the log message to the console.
    /// </summary>
    Console = 2, 
    /// <summary>
    /// Prints the log message to the file system.
    /// </summary>
    File = 4
}