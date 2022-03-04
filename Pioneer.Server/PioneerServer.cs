using Pioneer.Common.Commands;
using Pioneer.Common.Logging;
using Pioneer.Net;
using Pioneer.Net.Packet;
using Pioneer.Net.Protocol;
using Pioneer.Net.Protocol.Client;
using Pioneer.Server.Configuration;

namespace Pioneer.Server;

internal class PioneerServer
{
    internal static PioneerServer Instance { get; private set; } = null!;

    internal ILogger Logger { get; }
        
    internal Config Config { get; }
    
    internal ICommandRegistry CommandRegistry { get; }

    internal IServer? Server { get; private set; }

    private bool _isMainThreadLocked;

    internal PioneerServer(ILogger logger, Config config)
    {
        var o = new RequestAuthPacket();
        Instance = this;
        Logger = logger;
        Config = config;
        CommandRegistry = ICommandRegistry.Create();
        CommandRegistry.Scan(this);
    }
    
    internal async Task StartAsync()
    {
        _isMainThreadLocked = true;
        Logger.Info("Starting Pioneer Server on {IP}:{PORT}...", Config.IsLocal ? "127.0.0.1" : "0.0.0.0", Config.Port);
        if(!string.IsNullOrEmpty(Config.Password))
            Logger.Warn("Password Authentication is enabled!");
        Logger.Info("Initializing underlying Server Framework...");
        Server = await Network.CreateAsync(Config.IsLocal, Config.Port, Config.Password);
        if (Server != null)
        {
            Server.Disconnect += OnClientDisconnect;
            Server.Connect += OnClientConnect;
            Logger.Info("Pioneer was successfully started!");
            Logger.Debug("Waiting for incoming Connections...");
        }
        else
        {
            Logger.Fatal("Initialization of the Pioneer Server failed!");
        }
    }

    internal Task StopAsync()
    {
        Logger.Warn("Stopping Pioneer Server...");
        return Server?.StopAsync() ?? Task.CompletedTask;
    }
    
    internal void LockThread()
    {
        while (_isMainThreadLocked)
        {
            Thread.Sleep(0);
        }
    }

    internal void UnlockThread()
    {
        _isMainThreadLocked = false;
    }

    private void OnClientConnect(IClientConnection client)
    {
        Logger.Debug("Client #{ID} successfully established a connection!", client.ID);
    }

    private void OnClientDisconnect(IClientConnection client)
    {
        Logger.Debug("Client #{ID} lost connection!", client.ID);
    }

    [PacketHandler(typeof(EchoPacket))]
    public void OnEchoPacket(IClientConnection client, EchoPacket packet)
    {
        Logger.Debug("Echo received from Client#{ID}: {ME}", client.ID, packet.Message);
        client.SendPacket(packet);
    }

    [Command("help", "Shows information about all commands")]
    public void OnHelpCommand()
    {
        foreach (var command in CommandRegistry.GetCommandUsages())
        {
            Console.WriteLine(command);
        }
    }

    [Command("stop", "Stops the server")]
    public void OnStopCommand()
    {
        Program.StopProgram();
    }
}