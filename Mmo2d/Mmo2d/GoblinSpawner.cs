﻿using Mmo2d.EntityStateUpdates;
using Newtonsoft.Json;
using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d
{
    public class GoblinSpawner
    {
        public const int MaxGoblins = 5;
        public const float SpawningRadius = 15.0f;
        public static readonly TimeSpan SpawningInterval = TimeSpan.FromMilliseconds(2000.0);

        public List<Entity> SpawnedGoblins { get; set; }

        public float X, Y;

        public TimeSpan TimeSinceLastGoblinAddition { get; set; }

        [JsonIgnore]
        public Random Random { get; set; }

        public GoblinSpawner(Vector2 position, Random random)
        {
            SpawnedGoblins = new List<Entity>();
            X = position.X;
            Y = position.Y;
            TimeSinceLastGoblinAddition = SpawningInterval;
            Random = random;
        }

        public void Update(TimeSpan delta, List<Entity> entities, AggregateEntityStateUpdate updates)
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
                var newlySpawnedGoblin = new Entity() { IsGoblin = true, Id = Random.Next(), Hp = 10, /*EntityController = new GoblinEntityController(Random),*/ };

                var randomAngle = Random.NextDouble() * Math.PI * 2.0;
                var randomRadius = Random.NextDouble() * SpawningRadius;
                
                var randomCoords = System.Numerics.Complex.FromPolarCoordinates(Math.Sqrt(randomRadius), randomAngle);

                var randomPostion = new Vector2((float)randomCoords.Real + X, (float)randomCoords.Imaginary + Y);

                newlySpawnedGoblin.Location = randomPostion;

                TimeSinceLastGoblinAddition = TimeSpan.Zero;
                
                SpawnedGoblins.Add(newlySpawnedGoblin);

                updates[newlySpawnedGoblin.Id].Add = newlySpawnedGoblin;
            }
        }
    }
}
