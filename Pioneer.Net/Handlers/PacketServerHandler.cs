using DotNetty.Buffers;
using DotNetty.Transport.Channels;
using Pioneer.Net.Packet;
using Pioneer.Net.Protocol.Client;

namespace Pioneer.Net.Handlers;

internal class PacketServerHandler : ChannelHandlerAdapter
{
    private bool _isAuthenticated;
    private readonly Server _server;
    private readonly string? _password;

    internal PacketServerHandler(Server server, string? password)
    {
        _server = server;
        _password = password;
        _isAuthenticated = false;
    }

    public override void ChannelInactive(IChannelHandlerContext context)
    {
        var client = _server.GetClientByChannel(context.Channel);
        if (client != null)
        {
            _server.TriggerClientDisconnect(client);
        }
    }

    public override void ChannelRead(IChannelHandlerContext context, object message)
    {
        if (message is IByteBuffer buffer)
        {
            var packet = PacketRegistry.ConvertFromByteBuffer(buffer);
            if (packet != null)
            {
                if (!_isAuthenticated)
                {
                    if (packet is RequestAuthPacket authPacket)
                    {
                        if (string.IsNullOrEmpty(_password) || _password == authPacket.Password)
                        {
                            _isAuthenticated = true;
                            _server.TriggerClientConnect(context.Channel);
                            return;
                        }
                    }

                    context.CloseAsync();
                }
                else
                {
                    var client = _server.GetClientByChannel(context.Channel);
                    if (client != null)
                    {
                        PacketHandler.CallHandler(client, packet);
                    }
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