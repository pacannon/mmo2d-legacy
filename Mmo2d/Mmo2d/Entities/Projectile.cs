using Mmo2d.EntityStateUpdates;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Mmo2d.Entities
{
    public class ProjectileType
    {
        public static ProjectileType Fireball = new ProjectileType(4, 15, Color.Red, TimeSpan.FromSeconds(3.0), false);
        public static ProjectileType Frostbolt = new ProjectileType(3, 12, Color.Blue, TimeSpan.FromSeconds(2.5), true);

        public int Damage { get; set; }
        public float Range { get; set; }
        public Color Color { get; set; }
        public TimeSpan CastTime { get; set; }
        public bool? Chills { get; set; }

        public ProjectileType(int damage, float range, Color color, TimeSpan castTime, bool? chills)
        {
            Damage = damage;
            Range = range;
            Color = color;
            CastTime = castTime;
            Chills = chills;
        }
    }

    public class Projectile
    {
        public const float radius = 0.05f;

        public long Id { get; set; }
        public Vector2 Location { get; set; }
        public long TargetId { get; set; }
        public long LauncherId { get; set; }
        public TimeSpan Age { get; set; }

        public ProjectileType ProjectileType { get; set; }

        public Projectile(ProjectileType projectileType, Vector2 location, long targetId, long id, long launcherId)
        {
            ProjectileType = projectileType;

            TargetId = targetId;
            Location = location;
            Age = TimeSpan.Zero;
            Id = id;
            LauncherId = launcherId;
        }

        public void Render()
        {
            GL.Color3(ProjectileType.Color);

            int i;
            int triangleAmount = 1000;
            float twicePi = 2.0f * (float)Math.PI;

            GL.Enable(EnableCap.LineSmooth);
            GL.LineWidth(1.0f);

            GL.Begin(PrimitiveType.Lines);

            for (i = 0; i <= triangleAmount; i++)
            {
                GL.Vertex2(Location);
                GL.Vertex2(Location.X + (radius * Math.Cos(i * twicePi / triangleAmount)), Location.Y + (radius * Math.Sin(i * twicePi / triangleAmount)));
            }

            GL.End();

            GL.Disable(EnableCap.LineSmooth);
        }

        public void GenerateUpdates(TimeSpan delta, List<Entity> entities, AggregateEntityStateUpdate updates)
        {
            var nextLocation = NextLocation(delta, entities);

            if (nextLocation != null)
            {
                var targetEntity = TargetEntity(entities);

                if (targetEntity.Overlapping(nextLocation.Value))
                {
                    updates[targetEntity.Id].HpDeltas.Add(-ProjectileType.Damage);
                    updates[targetEntity.Id].SetTargetId = LauncherId;
                    updates[Id].RemoveProjectile = true;

                    if (ProjectileType.Chills.GetValueOrDefault())
                    {
                        updates[targetEntity.Id].ApplyChill = true;
                    }

                    if (targetEntity.Hp + updates[targetEntity.Id].HpDeltas.Sum() < 1)
                    {
                        updates[targetEntity.Id].Died = true;
                        updates[targetEntity.Id].Remove = true;
                        updates[LauncherId].KillsDelta = (updates[LauncherId].KillsDelta.HasValue ? updates[LauncherId].KillsDelta.Value : 0) + 1;
                    }
                }
            }

            else
            {
                updates[Id].RemoveProjectile = true;
            }
        }

        public void ApplyUpdate(TimeSpan delta, IEnumerable<Entity> entities) 
        {
            var nextLocation = NextLocation(delta, entities);

            if (nextLocation != null)
            {
                Location = nextLocation.Value;
            }

            Age += delta;
        }

        public Vector2? NextLocation(TimeSpan delta, IEnumerable<Entity> entities)
        {
            var targetEntity = TargetEntity(entities);

            if (targetEntity != null)
            {
                var t_squared = ((float)Age.TotalSeconds * (float)Age.TotalSeconds);

                var displacement = (targetEntity.Location - Location);

                var direction = displacement.Normalized();

                var newLocation = Location + Vector2.Multiply(direction, 0.04f + 0.3f * t_squared);

                if ((newLocation - Location).Length > (targetEntity.Location - Location).Length)
                {
                    newLocation = targetEntity.Location;
                }

                return newLocation;
            }

            return null;
        }

        public Entity TargetEntity(IEnumerable<Entity> entities)
        {
            var targetEntity = entities.FirstOrDefault(e => e.Id == TargetId);

            return targetEntity;
        }
    }
}
