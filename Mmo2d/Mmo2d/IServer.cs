using Mmo2d.AuthoritativePackets;
using Mmo2d.UserCommands;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public interface IServer
    {
        void QueueUserCommand(UserCommand userCommand);
        ConcurrentQueue<AuthoritativePacket> AuthoritativePacketQueue { get; }
    }
}
