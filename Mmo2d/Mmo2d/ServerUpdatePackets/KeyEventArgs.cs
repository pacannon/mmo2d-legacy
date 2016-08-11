using Newtonsoft.Json;
using OpenTK.Input;

namespace Mmo2d.ServerUpdatePackets
{
    public class KeyEventArgs
    {
        public bool? KeyUp { get; set; }
        
        [JsonIgnore]
        public bool? KeyDown { get { return KeyUp.HasValue ? (bool?)!KeyUp.Value : null; } }

        public Key? Key { get; set; }

        public bool? IsRepeat { get;  set; }
    }
}
