using System;
using System.IO;

namespace Pioneer.Common.Logging;

/// <summary>
/// The logger factory is the entry point for any logger. From here specific things are being managed
/// and it offers the creating of loggers.
/// </summary>
public static class LoggerFactory
{
    /// <summary>
    /// The base path of the folder containing the log files.
    /// </summary>
    public static string LoggerBasePath { get; set; } = Path.Combine(Environment.CurrentDirectory, "logs");

    /// <summary>
    /// Creates a logger with the given prefix as indicator. The prefix will also be used for the log file sub older.
    /// </summary>
    /// <param name="prefix">The prefix of the logger</param>
    /// <param name="debug">If true, the debug mode is enabled which ends in printing out of debug logs</param>
    /// <param name="configuration">The configuration of this logger instance</param>
    /// <returns>The newly created logger</returns>
    public static ILogger Create(string prefix, bool debug = false,
        LogConfiguration configuration = LogConfiguration.Console | LogConfiguration.File)
    {
        string path = Path.Combine(LoggerBasePath, prefix);
        Directory.CreateDirectory(path);
        return new Logger(prefix, path, debug, configuration);
    }
}