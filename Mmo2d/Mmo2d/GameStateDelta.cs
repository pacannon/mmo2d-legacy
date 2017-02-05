using Mmo2d.EntityStateUpdates;
using Newtonsoft.Json;
using System.Collections.Generic;

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
                return AggregateEntityStateUpdate.Count + EntitiesToRemove.Count + EntitiesToAdd.Count > 0;
            }
        }

        public GameStateDelta()
        {
            AggregateEntityStateUpdate = new AggregateEntityStateUpdate();
            EntitiesToRemove = new List<long>();
            EntitiesToAdd = new List<Entity>();
        }

        public bool ShouldSerializeEntityStateUpdates()
        {
            return (AggregateEntityStateUpdate.Count != 0);
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
