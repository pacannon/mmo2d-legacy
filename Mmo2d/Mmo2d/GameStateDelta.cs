using Newtonsoft.Json;
using System.Collections.Generic;

namespace Mmo2d
{
    public class GameStateDelta
    {
        public List<EntityStateUpdate> EntityStateUpdates { get; set; }
        public List<long> EntitiesToRemove { get; set; }
        public List<Entity> EntitiesToAdd { get; set; }

        [JsonIgnore]
        public bool ContainsInformation
        {
            get
            {
                return EntityStateUpdates.Count + EntitiesToRemove.Count + EntitiesToAdd.Count > 0;
            }
        }

        public GameStateDelta()
        {
            EntityStateUpdates = new List<EntityStateUpdate>();
            EntitiesToRemove = new List<long>();
            EntitiesToAdd = new List<Entity>();
        }

        public bool ShouldSerializeEntityStateUpdates()
        {
            return (EntityStateUpdates.Count != 0);
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
