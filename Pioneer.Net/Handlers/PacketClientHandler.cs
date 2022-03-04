using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Pioneer.Net.Packet;
using Pioneer.Net.Protocol.Client;
using Pioneer.Net.Protocol.Server;

namespace Pioneer.Net.Handlers;

internal class PacketClientHandler : ChannelHandlerAdapter
{
    private bool _isFinished;
    private readonly Client _client;
    private readonly string _password;
    private readonly Action<IClientConnection> _promise;

    internal PacketClientHandler(Client client, Action<IClientConnection> promise, string password)
    {
        _client = client;
        _promise = promise;
        _password = password;
        _isFinished = false;
    }

    public override void ChannelInactive(IChannelHandlerContext context) => _client.TriggerDisconnect();

    public override void ChannelActive(IChannelHandlerContext context) =>
        context.WriteAndFlushAsync(PacketRegistry.ConvertToByteBuffer(new RequestAuthPacket(_password)));

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IByteBuffer buffer)
        {
            var packet = PacketRegistry.ConvertFromByteBuffer(buffer);
            if (packet != null)
            {
                if (packet is AuthFinishPacket finishPacket)
                {
                    _client.ID = finishPacket.ID;
                    _isFinished = true;
                    _promise.Invoke(_client);
                }
                else if (_isFinished)
                {
                    PacketHandler.CallHandler(packet);
                }
            } 
        }
    }

    public override void ChannelReadComplete(IChannelHandlerContext context) => context.Flush();

    public override void ExceptionCaught(IChannelHandlerContext context, Exception exception)
    {
        LoggingInterface.ThrowError(exception);
        context.CloseAsync();
    }
}