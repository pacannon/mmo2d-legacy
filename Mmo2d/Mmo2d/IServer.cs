using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    interface IServer
    {
        void SendMessage(ServerMessage message);
        ConcurrentQueue<ServerResponse> ResponseQueue { get; }
    }

    public class ServerMessage
    {
        public char TypedCharacter { get; set; }
    }

    public class ServerResponse
    {
        public char TypedCharacter { get; set; }
    }
}
