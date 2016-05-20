using Mmo2d.ServerMessages;
using Mmo2d.ServerResponses;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    interface IServer
    {
        void SendMessage(IServerMessage message);
        ConcurrentQueue<IServerResponse> ResponseQueue { get; }
    }
}
