using Newtonsoft.Json;

namespace Mmo2d.UserCommands
{
    public class UserCommand
    {
        [JsonIgnore]
        public long? PlayerId { get; set; }

        public KeyEventArgs KeyEventArgs { get; set; }
        public bool? MousePressed { get; set; }

        public long? SetTargetId { get; set; }
        public bool? DeselectTarget { get; set; }

        // Not accepting this from the wire...
        [JsonIgnore]
        public Entity CreateEntity { get; internal set; }

        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }
    }
}
