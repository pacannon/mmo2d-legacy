using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class GoblinSpawner
    {
        public const int MaxGoblins = 4;
        public const float SpawningRadius = 1.0f;
        public static readonly TimeSpan SpawningInterval = TimeSpan.FromSeconds(1.0);

        public List<Entity> SpawnedGoblins { get; set; }

        public float X, Y;

        public TimeSpan TimeSinceLastGoblinAddition { get; set; }

        [JsonIgnore]
        public Random Random { get; set; }

        public GoblinSpawner(Vector2 position)
        {
            SpawnedGoblins = new List<Entity>();
            X = position.X;
            Y = position.Y;
            TimeSinceLastGoblinAddition = TimeSpan.Zero;
            Random = new Random();
        }

        public void Update(TimeSpan delta, List<Entity> entities)
        { 
            TimeSinceLastGoblinAddition += delta;

            foreach (var spawnedGoblin in SpawnedGoblins.ToList())
            {
                if (!entities.Any(e => e.Id == spawnedGoblin.Id))
                {
                    SpawnedGoblins.Remove(spawnedGoblin);
                }
            }

            if (SpawnedGoblins.Count < MaxGoblins && TimeSinceLastGoblinAddition >= SpawningInterval)
            {
                var newlySpawnedGoblin = new Entity() { OverriddenColor = Entity.GoblinColor, Id = Random.Next()};

                var randomAngle = Random.NextDouble() * Math.PI * 2.0;
                var randomRadius = Random.NextDouble() * SpawningRadius;

                // This is not uniform randomness. See http://mathworld.wolfram.com/DiskPointPicking.html for more info.
                var randomCoords = Complex.FromPolarCoordinates(randomRadius, randomAngle);
                newlySpawnedGoblin.x = (float)randomCoords.Real + X;
                newlySpawnedGoblin.y = (float)randomCoords.Imaginary + Y;

                TimeSinceLastGoblinAddition = TimeSpan.Zero;

                entities.Add(newlySpawnedGoblin);
                SpawnedGoblins.Add(newlySpawnedGoblin);
            }
        }
    }
}
