using Newtonsoft.Json;
using System;
using System.IO;

namespace Mmo2d.AuthoritativePackets
{
    public class AuthoritativePacket
    {
        public long? IdIssuance { get; set; }
        public State State { get; set; }

        public string ToJsonString()
        {
            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            writer.WriteStartObject();

            writer.WritePropertyName("IdIssuance");
            writer.WriteValue(IdIssuance);

            if (State != null)
            {
                writer.WritePropertyName("State");
                State.ToJsonString(writer);
            }
            writer.WriteEndObject();


            return sw.ToString();
        }
    }
}
