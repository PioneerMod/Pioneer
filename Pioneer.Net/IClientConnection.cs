using DotNetty.Transport.Channels;
using Pioneer.Net.Packet;

namespace Pioneer.Net;

/// <summary>
/// The client connection is the main component of the network belonging to a single player. A client contains mostly a connection
/// to the underlying system. If the client is used on the server you can send a packet to the client's machine,
/// otherwise from the machine.
/// </summary>
public interface IClientConnection
{
    /// <summary>
    /// The id of the client's connection in the current context. The id is given from the server and
    /// is runtime-unique.
    /// </summary>
    long ID { get; }
    
    /// <summary>
    /// The network channel associated to this client.
    /// </summary>
    IChannel Channel { get; }
    
    /// <summary>
    /// Gets called when client has been disconnected.
    /// </summary>
    event Action Disconnect;

    /// <summary>
    /// Sends a packet over the clients connection either to the server - when on client's machine - or
    /// from the server to the client's machine.
    /// </summary>
    /// <param name="packet">The packet to be sent.</param>
    void SendPacket(IPacket packet);
    
    /// <summary>
    /// Closes the connection and cleans up the garbage.
    /// </summary>
    Task CloseAsync();
}