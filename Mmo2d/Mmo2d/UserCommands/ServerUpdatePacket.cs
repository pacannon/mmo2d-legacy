using Newtonsoft.Json;

namespace Mmo2d.UserCommands
{
    public class UserCommand
    {
        public static readonly UserCommand NoChange = new UserCommand();

        [JsonIgnore]
        public long? PlayerId { get; set; }

        public KeyEventArgs KeyEventArgs { get; set; }
        public bool? MousePressed { get; set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
