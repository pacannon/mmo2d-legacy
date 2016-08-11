using System.Collections.Generic;

namespace Mmo2d.ServerUpdatePackets
{
    public class ServerUpdatePacket
    {
        public long? PlayerId { get; set; }

        public KeyEventArgs KeyEventArgs { get; set; }
        public bool? MousePressed { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
