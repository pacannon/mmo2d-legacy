using Newtonsoft.Json;
using System;

namespace Mmo2d.AuthoritativePackets
{
    public class AuthoritativePacket
    {
        public long? IdIssuance { get; set; }
        public State State { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
