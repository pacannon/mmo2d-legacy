using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d
{
    public class GameState
    {
        public List<Entity> Entities { get; set; }

        [JsonIgnore]
        public GoblinSpawner GoblinSpawner { get; set; }

        public GameState()
        {
            Entities = new List<Entity>();
        }

        public void Render()
        {
            foreach (var entity in Entities)
            {
                entity.Render();
            }
        }

        public GameStateDelta GenerateUpdate(TimeSpan delta)
        {
            var gameStateDelta = new GameStateDelta { Delta = delta };

            var entitiesCopy = Entities.ToList();
            var updates = new List<EntityStateUpdate>();

            foreach (var entity in entitiesCopy)
            {
                var generatedUpdates = entity.GenerateUpdates(delta, Entities);

                updates.AddRange(generatedUpdates);
            }

            gameStateDelta.EntityStateUpdates.AddRange(updates);

            gameStateDelta.EntitiesToRemove.AddRange(entitiesCopy.Where(e => e.TimeSinceDeath != null).Select(e => e.Id));

            if (GoblinSpawner != null)
            {
                gameStateDelta.EntitiesToAdd.AddRange(GoblinSpawner.Update(delta, Entities));
            }

            return gameStateDelta;
        }

        public void ApplyUpdates(IEnumerable<GameStateDelta> updates)
        {
            foreach (var update in updates)
            {
                if (update.EntityStateUpdates != null)
                {
                    var entitiesCopy = Entities.ToList();

                    foreach (var entity in entitiesCopy)
                    {
                        entity.ApplyUpdates(update.EntityStateUpdates.Where(u => u.EntityId == entity.Id));
                    }
                }

                Entities.RemoveAll(e => update.EntitiesToRemove.Contains(e.Id));

                Entities.AddRange(update.EntitiesToAdd);
            }
        }

        public GameState Clone()
        {
            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            //var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            var serialized = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<GameState>(serialized);
        }
    }
}
