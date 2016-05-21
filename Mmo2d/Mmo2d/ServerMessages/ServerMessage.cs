using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mmo2d.ServerMessages
{
    public abstract class ServerMessage
    {
        private Guid messageType;

        public Guid MessageType
        {
            get
            {
                return messageType;
            }

            set
            {
                if (this.messageType != value)
                {
                    throw new JsonException();
                }
            }
        }

        //protected ServerMessage(Guid messageType)
        //{
        //    this.messageType = messageType;
        //}
    }
}
