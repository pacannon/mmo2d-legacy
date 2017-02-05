using Mmo2d.Controller;
using Mmo2d.Entities;
using Mmo2d.EntityStateUpdates;
using Newtonsoft.Json;
using OpenTK;
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

        public void Render(EntityController playerController)
        {
            foreach (var entity in Entities)
            {
                entity.Render(entity.Id == playerController.TargetId);
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
            var updates = new AggregateEntityStateUpdate();

            foreach (var entity in entitiesCopy)
            {
                entity.GenerateUpdates(Entities, updates, Random);
            }

            foreach (var fireball in Fireballs)
            {
                fireball.GenerateUpdates(delta, entitiesCopy, updates);
            }

            gameStateDelta.EntitiesToRemove.AddRange(updates.Select(u => u.Value).Where(e => e.Died != null).Select(e => e.EntityId));

            if (GoblinSpawner != null)
            {
                gameStateDelta.EntitiesToAdd.AddRange(GoblinSpawner.Update(delta, Entities));
            }
            
            gameStateDelta.AggregateEntityStateUpdate = updates;

            return gameStateDelta;
        }

        public void ApplyUpdates(IEnumerable<GameStateDelta> updates, TimeSpan delta)
        {
            Entities.AddRange(updates.SelectMany(u => u.EntitiesToAdd));

            var entitiesCopy = Entities.ToList();
            var entityStateUpdates = updates.SelectMany(u => u.AggregateEntityStateUpdate);

            foreach (var entity in entitiesCopy)
            {
                entity.ApplyUpdates(entityStateUpdates.Where(u => u.Key == entity.Id).Select(u => u.Value), delta);
            }

            foreach (var newFireball in entityStateUpdates.Where(u => u.Value.AddFireball != null).Select(u => u.Value.AddFireball))
            {
                Fireballs.Add(newFireball);
            }

            foreach (var fireball in Fireballs)
            {
                fireball.ApplyUpdate(delta, entitiesCopy);
            }

            foreach (var removeId in entityStateUpdates.Where(u => u.Value.RemoveFireball != null))
            {
                Fireballs.RemoveAll(f => f.Id == removeId.Value.EntityId);
            }

            var removed = Entities.RemoveAll(e => updates.SelectMany(u => u.EntitiesToRemove).Contains(e.Id));

            Entities = Entities.OrderBy(e => e.Location.X).OrderByDescending(e => e.Location.Y).ToList();

            foreach (var entity in Entities.Where(e => e.TargetId.HasValue && !(Entities.Select(t => t.Id).Contains(e.TargetId.Value))))
            {
                entity.EntityController.TargetId = null;
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

        internal long? TargetId(Vector2 targetLocation)
        {
            var entitiesCopy = Entities.ToList();
            entitiesCopy.Reverse();

            foreach (var entitiy in entitiesCopy)
            {
                if (entitiy.Overlapping(targetLocation))
                {
                    return entitiy.Id;
                }
            }

            return null;
        }
    }
}
