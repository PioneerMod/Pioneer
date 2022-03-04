using System.Net;
using DotNetty.Codecs;
using DotNetty.Handlers.Logging;
using DotNetty.Transport.Bootstrapping;
using DotNetty.Transport.Channels;
using DotNetty.Transport.Channels.Sockets;
using Pioneer.Net.Handlers;
using Pioneer.Net.Packet;

namespace Pioneer.Net;

internal class Client : IClientConnection
{
    public long ID { get; internal set; }
    public IChannel Channel { get; private set; } = null!;
    
    public event Action? Disconnect;

    private readonly IEventLoopGroup _group = null!;
    private readonly string _serverIp = null!;
    private readonly int _serverPort;
    private readonly string _serverPassword = null!;

    internal Client(long id, IChannel channel)
    {
        ID = id;
        Channel = channel;
    }

    internal Client(string ip, int port, string password)
    {
        _serverIp = ip;
        _serverPort = port;
        _serverPassword = password;
        _group = new MultithreadEventLoopGroup();
    }
    
    public void SendPacket(IPacket packet)
    {
        Channel.WriteAndFlushAsync(PacketRegistry.ConvertToByteBuffer(packet));
    }

    public async Task CloseAsync()
    {
        await Channel.CloseAsync();
    }

    internal void TriggerDisconnect()
    {
        Disconnect?.Invoke();
    }

    internal async Task ConnectServer(Action<IClientConnection> promise)
    {
        var bootstrap = new Bootstrap();
        bootstrap.Group(_group)
            .Channel<TcpSocketChannel>()
            .Option(ChannelOption.TcpNodelay, true)
            .Handler(new ActionChannelInitializer<ISocketChannel>(channel =>
            {
                channel.Pipeline.AddLast(new LoggingHandler());
                channel.Pipeline.AddLast("framing-enc", new LengthFieldPrepender(2));
                channel.Pipeline.AddLast("framing-dec", new LengthFieldBasedFrameDecoder(ushort.MaxValue, 0, 2, 0, 2));
                channel.Pipeline.AddLast("pioneer-packet", new PacketClientHandler(this, promise, _serverPassword));
            }));
        Channel = await bootstrap.ConnectAsync(new IPEndPoint(IPAddress.Parse(_serverIp), _serverPort));
    }
}