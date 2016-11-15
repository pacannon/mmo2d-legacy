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
            var gameStateDelta = new GameStateDelta();

            var entitiesCopy = Entities.ToList();
            var updates = new List<EntityStateUpdate>();

            foreach (var entity in entitiesCopy)
            {
                var generatedUpdates = entity.GenerateUpdates(Entities);

                updates.AddRange(generatedUpdates);
            }

            gameStateDelta.EntityStateUpdates.AddRange(updates);

            gameStateDelta.EntitiesToRemove.AddRange(updates.Where(e => e.Died != null).Select(e => e.EntityId));

            if (GoblinSpawner != null)
            {
                gameStateDelta.EntitiesToAdd.AddRange(GoblinSpawner.Update(delta, Entities));
            }

            return gameStateDelta;
        }

        public void ApplyUpdates(IEnumerable<GameStateDelta> updates, TimeSpan delta)
        {
            Entities.AddRange(updates.SelectMany(u => u.EntitiesToAdd));

            var entitiesCopy = Entities.ToList();
            var entityStateUpdates = updates.SelectMany(u => u.EntityStateUpdates);

            foreach (var entity in entitiesCopy)
            {
                entity.ApplyUpdates(entityStateUpdates.Where(u => u.EntityId == entity.Id), delta);
            }

            Entities.RemoveAll(e => updates.SelectMany(u => u.EntitiesToRemove).Contains(e.Id));
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
