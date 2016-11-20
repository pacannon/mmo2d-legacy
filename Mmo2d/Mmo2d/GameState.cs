using Mmo2d.Entities;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d
{
    public class GameState
    {
        public List<Entity> Entities { get; set; }
        public List<Fireball> Fireballs { get; set; }

        [JsonIgnore]
        public Random Random { get; set; }

        [JsonIgnore]
        public GoblinSpawner GoblinSpawner { get; set; }

        public GameState(Random random)
        {
            Entities = new List<Entity>();
            Fireballs = new List<Fireball>();
            Random = random;
        }

        public void Render()
        {
            foreach (var entity in Entities)
            {
                entity.Render();
            }

            foreach (var fireball in Fireballs)
            {
                fireball.Render();
            }
        }

        public GameStateDelta GenerateUpdate(TimeSpan delta)
        {
            var gameStateDelta = new GameStateDelta();

            var entitiesCopy = Entities.ToList();
            var updates = new List<EntityStateUpdate>();

            foreach (var entity in entitiesCopy)
            {
                var generatedUpdates = entity.GenerateUpdates(Entities, Random);

                updates.AddRange(generatedUpdates);
            }

            foreach (var fireball in Fireballs)
            {
                var generatedupdates = fireball.GenerateUpdates(delta, entitiesCopy);
                updates.AddRange(generatedupdates);
            }

            gameStateDelta.EntitiesToRemove.AddRange(updates.Where(e => e.Died != null).Select(e => e.EntityId));

            if (GoblinSpawner != null)
            {
                gameStateDelta.EntitiesToAdd.AddRange(GoblinSpawner.Update(delta, Entities));
            }
            
            gameStateDelta.EntityStateUpdates.AddRange(updates);

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

            foreach (var newFireball in entityStateUpdates.Where(u => u.AddFireball != null).Select(u => u.AddFireball))
            {
                Fireballs.Add(newFireball);
            }

            foreach (var fireball in Fireballs)
            {
                fireball.ApplyUpdate(delta, entitiesCopy);
            }

            foreach (var removeId in entityStateUpdates.Where(u => u.RemoveFireball != null))
            {
                Fireballs.RemoveAll(f => f.Id == removeId.EntityId);
            }

            var removed = Entities.RemoveAll(e => updates.SelectMany(u => u.EntitiesToRemove).Contains(e.Id));


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
