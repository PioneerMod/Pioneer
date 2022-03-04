using Serilog;
using Serilog.Core;
using Serilog.Events;
using SLogger = Serilog.Core.Logger;

namespace Pioneer.Common.Logging;

internal class Logger : ILogger
{
    private readonly SLogger _internalLogger;
    private readonly string _path;
    private readonly string _prefix;
    private bool _isDebug;

    internal Logger(string prefix, string path, bool isDebug, LogConfiguration configuration)
    {
        _prefix = prefix;
        _path = path;
        _isDebug = isDebug;
        LoggerConfiguration loggerConfiguration = new LoggerConfiguration().MinimumLevel.Debug();
        if (configuration.HasFlag(LogConfiguration.Console))
            loggerConfiguration = loggerConfiguration.WriteTo.Console();
        if (configuration.HasFlag(LogConfiguration.File))
            loggerConfiguration = loggerConfiguration.WriteTo.Sink(new EventLogSink(this));
        _internalLogger = loggerConfiguration.CreateLogger();
    }

    private string Format(string message)
    {
        return $"[{_prefix}] {message}";
    }

    public void Info(string message, params object[] args)
    {
        _internalLogger.Information(Format(message), args);
    }

    public void Warn(string message, params object[] args)
    {
        _internalLogger.Warning(Format(message), args);
    }

    public void Verbose(string message, params object[] args)
    {
        _internalLogger.Verbose(Format(message), args);
    }

    public void Debug(string message, params object[] args)
    {
        if (!_isDebug) return;
        _internalLogger.Debug(Format(message), args);
    }

    public void Fatal(string message, params object[] args)
    {
        _internalLogger.Fatal(Format(message), args);
    }

    public void Error(Exception exception, string message, params object[] args)
    {
        _internalLogger.Fatal(Format(message) + "\n" + exception, args);
    }

    public void SetDebug(bool enabled)
    {
        _isDebug = enabled;
    }

    private class EventLogSink : ILogEventSink
    {
        private readonly ReaderWriterLock _locker = new();
        private readonly Logger _parent;

        internal EventLogSink(Logger parent)
        {
            _parent = parent;
        }

        public void Emit(LogEvent logEvent)
        {
            string line = $"[{DateTime.Now:hh:mm:ss} {LevelToSeverity(logEvent)}] " + logEvent.RenderMessage();
            try
            {
                _locker.AcquireWriterLock(int.MaxValue);
                File.AppendAllLines(Path.Combine(_parent._path, DateTime.Today.ToString("dd_MM_yyyy") + ".txt"),
                    new[] {line});
            }
            finally
            {
                _locker.ReleaseWriterLock();
            }
        }

        private string LevelToSeverity(LogEvent logEvent)
        {
            return logEvent.Level switch
            {
                LogEventLevel.Debug => "DBG",
                LogEventLevel.Error => "ERR",
                LogEventLevel.Fatal => "FTL",
                LogEventLevel.Verbose => "VRB",
                LogEventLevel.Warning => "WRN",
                _ => "INF"
            };
        }
    }
}