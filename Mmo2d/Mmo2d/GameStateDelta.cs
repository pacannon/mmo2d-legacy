using Mmo2d.EntityStateUpdates;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d
{
    public class GameStateDelta
    {
        public AggregateEntityStateUpdate AggregateEntityStateUpdate { get; set; }
        public List<long> EntitiesToRemove { get; set; }
        public List<Entity> EntitiesToAdd { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return ShouldSerializeAggregateEntityStateUpdate() || (EntitiesToRemove.Count + EntitiesToAdd.Count > 0);
            }
        }

        public GameStateDelta()
        {
            AggregateEntityStateUpdate = new AggregateEntityStateUpdate();
            EntitiesToRemove = new List<long>();
            EntitiesToAdd = new List<Entity>();
        }

        public bool ShouldSerializeAggregateEntityStateUpdate()
        {
            return (AggregateEntityStateUpdate.Any(u => u.Value.ContainsInformation));
        }

        public bool ShouldSerializeEntitiesToRemove()
        {
            return (EntitiesToRemove.Count != 0);
        }

        public bool ShouldSerializeEntitiesToAdd()
        {
            return (EntitiesToAdd.Count != 0);
        }
    }
}
