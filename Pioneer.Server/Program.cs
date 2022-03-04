using System.Globalization;
using System.Runtime.InteropServices;
using Nett;
using Pioneer.Common.Logging;
using Pioneer.Net;
using Pioneer.Net.Packet;
using Pioneer.Server.Configuration;
using Pioneer.Server.Utils;

namespace Pioneer.Server;

internal static class Program
{
    private static readonly TaskCompletionSource<bool> CancelKeyPress = new();
    private static NativeMethods.HandlerRoutine _windowsConsoleEventHandler = null!;
    private static PioneerServer _server = null!;
    private static bool _shutdownPending;
    
    private static async Task Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
        Console.Title = $"Pioneer Server (c) 2022-{DateTime.Today.Year} https://polaryx.de/";
        Console.CursorVisible = false;
        
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _windowsConsoleEventHandler += OnConsoleEvent;
            NativeMethods.SetConsoleCtrlHandler(_windowsConsoleEventHandler, true);
        }
        else
        {
            Console.CancelKeyPress += OnConsoleCancelKeyPressed!;
        }
        
        Console.WriteLine("Loading Server Configuration...");
        var config = TryLoadConfig();
        if (config == null) return;
        
        var logPath = Path.Combine(Environment.CurrentDirectory, "logs");
        LoggerFactory.LoggerBasePath = logPath;

        var logger = LoggerFactory.Create("Pioneer", config.IsDebug);
        LoggingInterface.Error += exception =>
        {
            logger.Error(exception, "An error occurred in the underlying Pioneer.Net runtime!");
        };

        logger.Debug("Initialize Packet Registry...");
        PacketRegistry.Initialize();

        InitConsoleInput();
        
        _server = new PioneerServer(logger, config);
        ConsoleIO.SetAutoCompleteEngine(new CommandAutoComplete());
        PacketHandler.Scan(_server);
        
        await _server.StartAsync();
        _server.LockThread();
    }

    internal static void StopProgram()
    {
        _shutdownPending = true;
        _server.StopAsync();
        _server.UnlockThread();
        _server.Logger.Info("Server successfully stopped!");
    }

    private static Config? TryLoadConfig()
    {
        var configPath = Path.Combine(Environment.CurrentDirectory, "config.toml");
        if (!File.Exists(configPath))
        {
            Toml.WriteFile(new Config(), configPath);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("New config.toml created! Please configure the server first!");
            Console.ResetColor();
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            return null;
        }

        try
        {
            return Toml.ReadFile<Config>(configPath);
        }
        catch
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("The existing config.toml is corrupted! Delete the config.toml and restart!");
            Console.ResetColor();
            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
            return null;
        }
    }

    private static void InitConsoleInput()
    {
        Task.Run(async () =>
        {
            await Task.Delay(2000);
            while (!_shutdownPending)
            {
                var input = ConsoleIO.ReadLine();
                if (!string.IsNullOrEmpty(input))
                {
                    if (!await _server.CommandRegistry.ProcessAsync(input))
                    {
                        _server.Logger.Warn("Type \"help\" for information about the commands!");
                    }
                }
            }
        });
    }

    private static void OnConsoleCancelKeyPressed(object sender, ConsoleCancelEventArgs e)
    {
        e.Cancel = true;
        StopProgram();
        CancelKeyPress.SetResult(true);
    }

    private static bool OnConsoleEvent(NativeMethods.CtrlType ctrlType)
    {
        Console.Write("\n");
        StopProgram();
        return true;
    }

    internal class CommandAutoComplete : IAutoComplete
    {
        public IEnumerable<string> AutoComplete(string behindCursor)
        {
            return _server.CommandRegistry.GetCommandNames().Where(command => command.ToLowerInvariant().StartsWith(behindCursor.ToLowerInvariant()));
        }
    }
}