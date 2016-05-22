using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.IO;
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

            if (GoblinSpawner != null)
            {
                GoblinSpawner.Update(delta, Entities);
            }
        }

        public State Clone()
        {
            // initialize inner objects individually
            // for example in default constructor some list property initialized with some values,
            // but in 'source' these items are cleaned -
            // without ObjectCreationHandling.Replace default constructor values will be added to result
            //var deserializeSettings = new JsonSerializerSettings { ObjectCreationHandling = ObjectCreationHandling.Replace };

            StringWriter sw = new StringWriter();
            JsonTextWriter writer = new JsonTextWriter(sw);

            ToJsonString(writer);

            var serialized = sw.ToString();
                
                
            return JsonSerializer.Deserialize<State>(serialized);
        }

        public void ToJsonString(JsonTextWriter writer)
        {

            // {
            writer.WriteStartObject();

            // "name" : "Jerry"
            writer.WritePropertyName("ElapsedTime");
            writer.WriteValue(ElapsedTime);

            writer.WritePropertyName("Updates");
            writer.WriteValue(Updates);

            // "likes": ["Comedy", "Superman"]
            writer.WritePropertyName("Entities");
            writer.WriteStartArray();
            foreach (var like in Entities)
            {
                like.ToJsonString(writer);
            }
            writer.WriteEndArray();

            // }
            writer.WriteEndObject();
        }
    }
}
