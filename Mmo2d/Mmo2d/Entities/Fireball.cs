using Mmo2d.EntityStateUpdates;
using Newtonsoft.Json;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Mmo2d.Entities
{
    public class Fireball
    {
        public const float width = 0.2f;
        public const float height = 0.2f;
        public const float radius = 0.05f;
        public static readonly TimeSpan CastTime = TimeSpan.FromSeconds(3.0);
        public const int damage = 6;
        
        [JsonIgnore]
        public float LeftEdge { get { return Location.X - width / 2.0f; } }
        [JsonIgnore]
        public float RightEdge { get { return Location.X + width / 2.0f; } }
        [JsonIgnore]
        public float TopEdge { get { return Location.Y + height / 2.0f; } }
        [JsonIgnore]
        public float BottomEdge { get { return Location.Y - height / 2.0f; } }
        [JsonIgnore]
        public Vector2 TopLeftCorner { get { return new Vector2(LeftEdge, TopEdge); } }
        [JsonIgnore]
        public Vector2 BottomLeftCorner { get { return new Vector2(LeftEdge, BottomEdge); } }
        [JsonIgnore]
        public Vector2 BottomRightCorner { get { return new Vector2(RightEdge, BottomEdge); } }
        [JsonIgnore]
        public Vector2 TopRightCorner { get { return new Vector2(RightEdge, TopEdge); } }

        public Fireball(Vector2 location, long targetId, long id, long launcherId)
        {
            TargetId = targetId;
            Location = location;
            Age = TimeSpan.Zero;
            Id = id;
            LauncherId = launcherId;
        }

        public void Render()
        {
            GL.Color3(Color.Red);

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
                    updates[targetEntity.Id].HpDelta = (updates[targetEntity.Id].HpDelta.HasValue ? updates[targetEntity.Id].HpDelta.Value : 0) - damage;
                    updates[targetEntity.Id].SetTargetId = LauncherId;
                    updates[Id].RemoveFireball = true;

                    if (targetEntity.Hp + updates[targetEntity.Id].HpDelta < 1)
                    {
                        updates[targetEntity.Id].Died = true;
                        updates[LauncherId].KillsDelta = (updates[LauncherId].KillsDelta.HasValue ? updates[LauncherId].KillsDelta.Value : 0) + 1;
                    }
                }
            }

            else
            {
                updates[Id].RemoveFireball = true;
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

        public long Id { get; set; }
        public Vector2 Location { get; set; }
        public long TargetId { get; set; }
        public long LauncherId { get; set; }
        public TimeSpan Age { get; set; }

        public Vector2? NextLocation(TimeSpan delta, IEnumerable<Entity> entities)
        {
            var targetEntity = TargetEntity(entities);

            if (targetEntity != null)
            {
                var t_squared = (0.03f) * ((float)Age.TotalSeconds * (float)Age.TotalSeconds);

                var direction = (targetEntity.Location - Location).Normalized();

                var newLocation = Location + Vector2.Multiply(direction, 0.04f) + Vector2.Multiply(direction, 10.0f * t_squared);

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
