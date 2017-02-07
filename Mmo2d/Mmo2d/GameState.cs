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
        public List<Projectile> Projectiles { get; set; }

        [JsonIgnore]
        public Random Random { get; set; }

        [JsonIgnore]
        public GoblinSpawner GoblinSpawner { get; set; }

        public GameState(Random random)
        {
            Entities = new List<Entity>();
            Projectiles = new List<Projectile>();
            Random = random;
        }

        public void Render(EntityController playerController)
        {
            foreach (var entity in Entities)
            {
                entity.Render(entity.Id == playerController.TargetId);
            }

            foreach (var fireball in Projectiles)
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

            foreach (var fireball in Projectiles)
            {
                fireball.GenerateUpdates(delta, entitiesCopy, updates);
            }

            if (GoblinSpawner != null)
            {
                GoblinSpawner.Update(delta, Entities, updates);
            }
            
            gameStateDelta.AggregateEntityStateUpdate = updates;

            return gameStateDelta;
        }

        public void ApplyUpdates(IEnumerable<GameStateDelta> updates, TimeSpan delta)
        {
            Entities.AddRange(updates.SelectMany(u => u.AggregateEntityStateUpdate).Where(u => u.Value.Add != null).Select(u => u.Value.Add));

            var entitiesCopy = Entities.ToList();
            var entityStateUpdates = updates.SelectMany(u => u.AggregateEntityStateUpdate);

            foreach (var entity in entitiesCopy)
            {
                entity.ApplyUpdates(entityStateUpdates.Where(u => u.Key == entity.Id).Select(u => u.Value), delta);
            }

            foreach (var newFireball in entityStateUpdates.Where(u => u.Value.AddFireball != null).Select(u => u.Value.AddFireball))
            {
                Projectiles.Add(newFireball);
            }

            foreach (var projectile in Projectiles)
            {
                projectile.ApplyUpdate(delta, entitiesCopy);
            }

            foreach (var removeId in entityStateUpdates.Where(u => u.Value.RemoveProjectile != null))
            {
                Projectiles.RemoveAll(f => f.Id == removeId.Value.EntityId);
            }

            var entitiesToRemove = updates.SelectMany(u => u.AggregateEntityStateUpdate).Where(u => u.Value.Remove != null).Select(u => u.Value.EntityId);

            var removed = Entities.RemoveAll(e => entitiesToRemove.Contains(e.Id));

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
