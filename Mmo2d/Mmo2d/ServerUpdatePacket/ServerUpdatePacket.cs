using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mmo2d.ServerUpdatePackets
{
    public class ServerUpdatePacket
    {
        public char? TypedCharacter { get; set; }

        public long? PlayerId { get; set; }

        public override string ToString()
        {
            string json = JsonConvert.SerializeObject(this, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            return json;
        }
    }
}
