using Newtonsoft.Json;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d.ServerUpdatePackets
{
    public class KeyEventArgs
    {
        public bool KeyUp { get; set; }
        
        [JsonIgnore]
        public bool KeyDown { get { return !KeyUp; } }

        public Key Key { get; set; }

        public bool IsRepeat { get;  set; }
    }
}
