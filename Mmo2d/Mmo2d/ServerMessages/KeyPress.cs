using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d.ServerMessages
{
    class KeyPress : IServerMessage
    {
        public char TypedCharacter { get; set; }
    }
}
