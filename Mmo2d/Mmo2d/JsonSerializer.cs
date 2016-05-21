using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mmo2d.AuthoritativePackets;

namespace Mmo2d
{
    public static class JsonSerializer
    {
        public static string Serialize(object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    ContractResolver = ShouldSerializeContractResolver.Instance,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                });

            return json;
        }

        public static T Deserialize<T>(string serializedAuthoritativePacket)
        {
            return JsonConvert.DeserializeObject<T>(serializedAuthoritativePacket);
        }
    }
}
