using Newtonsoft.Json;
using OpenTK.Input;

namespace Mmo2d.UserCommands
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
