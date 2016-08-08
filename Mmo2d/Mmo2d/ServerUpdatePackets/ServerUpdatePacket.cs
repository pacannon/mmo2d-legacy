using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenTK.Input;

namespace Mmo2d.ServerUpdatePackets
{
    public class ServerUpdatePacket
    {
        public long? PlayerId { get; set; }
        public KeyEventArgs KeyEventArgs { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
