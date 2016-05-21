using Mmo2d.AuthoritativePackets;
using Mmo2d.ServerUpdatePackets;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public interface IServer
    {
        void SendMessage(ServerUpdatePacket message);
        ConcurrentQueue<AuthoritativePacket> ResponseQueue { get; }
        void Update();
    }
}
