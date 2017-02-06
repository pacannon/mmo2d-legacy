using Mmo2d.EntityStateUpdates;
using Newtonsoft.Json;
using System.Linq;

namespace Mmo2d
{
    public class GameStateDelta
    {
        public AggregateEntityStateUpdate AggregateEntityStateUpdate { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return ShouldSerializeAggregateEntityStateUpdate();
            }
        }

        public GameStateDelta()
        {
            AggregateEntityStateUpdate = new AggregateEntityStateUpdate();
        }

        public bool ShouldSerializeAggregateEntityStateUpdate()
        {
            var should = (AggregateEntityStateUpdate.Any(u => u.Value.ContainsInformation));
            return should;
        }
    }
}
