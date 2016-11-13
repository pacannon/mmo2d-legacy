using Mmo2d.AuthoritativePackets;
using Mmo2d.ServerUpdatePackets;
using System.Collections.Concurrent;

namespace Mmo2d
{
    public interface IServer
    {
        void SendMessage(ServerUpdatePacket message);
        ConcurrentQueue<AuthoritativePacket> ResponseQueue { get; }

        uint Tick { get; set; }
    }
}
