using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using Mmo2d.State;
using Mmo2d.State.Entity;

namespace Mmo2d
{
    public class GameState : IStateful<GameState, EntityStateDifference, Entity>
    {
        public List<Entity> Entities { get; set; }

        public TimeSpan ElapsedTime { get; set; }

        public uint Tick { get; set; }

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

        public void Update(TimeSpan delta)
        {
            ElapsedTime += delta;

            Tick++;

            var entitiesCopy = Entities.ToList();

            foreach (var entity in entitiesCopy)
            {
                entity.Update(delta, Entities);
            }

            Entities.RemoveAll(e => e.TimeSinceDeath != null);

            GoblinSpawner?.Update(delta, Entities);
        }

        public object Clone()
        {
            var clone = new GameState();

            foreach (var entitiy in Entities)
            {
                clone.Entities.Add((Entity)entitiy.Clone());
            }

            clone.ElapsedTime = ElapsedTime;
            clone.Tick = Tick;
            clone.GoblinSpawner = GoblinSpawner.Clone();

            return clone;
        }
        
        public GameState Apply(IEnumerable<EntityStateDifference> differences)
        {
            var clone = (GameState)this.Clone();

            foreach (var difference in differences)
            {
                var entityStateDifference = difference as EntityStateDifference;

                if (entityStateDifference == null)
                {
                    return clone;
                }

                var matchingEntity = clone.Entities.First(e => e.Id == entityStateDifference.EntityId);

                var modifiedEntity = difference.Apply(matchingEntity);

                clone.Entities.Remove(matchingEntity);
                clone.Entities.Add(modifiedEntity);

                return clone;
            }

            return clone;
        }
    }
}
