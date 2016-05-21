using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class State
    {
        public List<Entity> Entities { get; set; }
        public GoblinSpawner GoblinSpawner { get; set; }
        public TimeSpan ElapsedTime { get; set; }

        public int Updates { get; set; }

        public State()
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

            Updates++;
               var entitiesCopy = Entities.ToList();

            foreach (var entity in entitiesCopy)
            {
                entity.Update(delta, Entities);
            }

            Entities.RemoveAll(new Predicate<Entity>((e) => e.TimeSinceDeath != null));

            GoblinSpawner.Update(delta, Entities);
        }

        public State Clone()
        {
            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            //var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            var serialized = JsonSerializer.Serialize(this);
            return JsonSerializer.Deserialize<State>(serialized);
        }
    }
}
