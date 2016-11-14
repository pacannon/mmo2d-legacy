using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Mmo2d
{
    public class GameStateDelta
    {
        public TimeSpan Delta { get; set; }

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
    }
}
