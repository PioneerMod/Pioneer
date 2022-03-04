using System.Net;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using DotNetty.Transport.Libuv;
using Pioneer.Net.Handlers;
using Pioneer.Net.Packet;
using Pioneer.Net.Protocol.Server;

namespace Pioneer.Net;

internal class Server : IServer
{
    public List<IClientConnection> Clients { get; }
    
    public int Port { get; }
    
    public event Action<IClientConnection>? Disconnect, Connect;

    private readonly bool _isLocal;
    private readonly string? _password;
    private readonly IEventLoopGroup _bossGroup;
    private readonly IEventLoopGroup _workerGroup;
    private IChannel _boundChannel;
    private long _currentId;

    internal Server(bool local, int port, string? password)
    {
        Clients = new List<IClientConnection>();
        Port = port;
        _currentId = 0;
        _isLocal = local;
        _password = password;

        if (Network.UseLibuv)
        {
            var dispatcher = new DispatcherEventLoopGroup();
            _bossGroup = dispatcher;
            _workerGroup = new WorkerEventLoopGroup(dispatcher);
        }
        else
        {
            _bossGroup = new MultithreadEventLoopGroup(Network.EventLoopCount);
            _workerGroup = new MultithreadEventLoopGroup();
        }
    }
    
    public async Task StartAsync()
    {
        var bootstrap = new ServerBootstrap();
        bootstrap.Group(_bossGroup, _workerGroup);

        if (Network.UseLibuv)
        {
            bootstrap.Channel<TcpServerChannel>();
        }
        else
        {
            bootstrap.Channel<TcpServerSocketChannel>();
        }

        bootstrap.Option(ChannelOption.SoBacklog, 100)
            .Handler(new LoggingHandler("SRV-LSTN"))
            .ChildHandler(new ActionChannelInitializer<IChannel>(channel =>
            {
                channel.Pipeline.AddLast(new LoggingHandler());
                channel.Pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                channel.Pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                channel.Pipeline.AddLast("pioneer-packet", new PacketServerHandler(this, _password));
            }));
        _boundChannel = await bootstrap.BindAsync(!_isLocal ? IPAddress.Any : IPAddress.Loopback, Port);
    }

    public async Task StopAsync()
    {
        await _boundChannel.CloseAsync();
        await _bossGroup.ShutdownGracefullyAsync();
        await _workerGroup.ShutdownGracefullyAsync();
    }

    public void BroadcastPacket(IPacket packet)
    {
        lock (Clients)
        {
            for (int i = Clients.Count - 1; i >= 0; i--)
            {
                Clients[i].SendPacket(packet);
            }
        }
    }

    public IClientConnection? GetClientByChannel(IChannel channel)
    {
        lock (Clients)
        {
            return Clients.FirstOrDefault(client => Equals(client.Channel.Id, channel.Id));
        }
    }

    public IClientConnection? GetClient(long id)
    {
        lock (Clients)
        {
            return Clients.FirstOrDefault(client => Equals(client.ID, id));
        }
    }

    internal void TriggerClientDisconnect(IClientConnection client)
    {
        lock (Clients)
        {
            Clients.Remove(client);
            Disconnect?.Invoke(client);
            ((Client) client).TriggerDisconnect();
        }
    }

    internal void TriggerClientConnect(IChannel channel)
    {
        var client = new Client(_currentId++, channel);
        lock (Clients)
        {
            Clients.Add(client);
            Connect?.Invoke(client);
        }
        
        client.SendPacket(new AuthFinishPacket(client.ID));
    } 
}