using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d
{
    public class GoblinSpawner
    {
        public const int MaxGoblins = 8;
        public const float SpawningRadius = 1.0f;
        public static readonly TimeSpan SpawningInterval = TimeSpan.FromMilliseconds(643.0);

        public List<long> SpawnedGoblinIds { get; set; }

        public float X, Y;

        public TimeSpan TimeSinceLastGoblinAddition { get; set; }

        [JsonIgnore]
        public Random Random { get; set; }

        public GoblinSpawner(Vector2 position)
        {
            SpawnedGoblinIds = new List<long>();
            X = position.X;
            Y = position.Y;
            TimeSinceLastGoblinAddition = TimeSpan.Zero;
            Random = new Random();
        }

        public void Update(TimeSpan delta, List<Entity> entities)
        { 
            TimeSinceLastGoblinAddition += delta;

            foreach (var spawnedGoblinId in SpawnedGoblinIds.ToList())
            {
                if (!entities.Any(e => e.Id == spawnedGoblinId))
                {
                    SpawnedGoblinIds.Remove(spawnedGoblinId);
                }
            }

            if (SpawnedGoblinIds.Count < MaxGoblins && TimeSinceLastGoblinAddition >= SpawningInterval)
            {
                var newlySpawnedGoblin = new Entity() { OverriddenColor = Entity.GoblinColor, Id = Random.Next()};

                var randomAngle = Random.NextDouble() * Math.PI * 2.0;
                var randomRadius = Random.NextDouble() * SpawningRadius;

                // This is not uniform randomness. See http://mathworld.wolfram.com/DiskPointPicking.html for more info.
                var randomCoords = System.Numerics.Complex.FromPolarCoordinates(randomRadius, randomAngle);

                var randomPostion = new Vector2((float)randomCoords.Real + X, (float)randomCoords.Imaginary + Y);

                newlySpawnedGoblin.Location = randomPostion;

                TimeSinceLastGoblinAddition = TimeSpan.Zero;

                entities.Add(newlySpawnedGoblin);
                SpawnedGoblinIds.Add(newlySpawnedGoblin.Id);
            }
        }

        public GoblinSpawner Clone()
        {
            return (GoblinSpawner)this.MemberwiseClone();
        }
    }
}
