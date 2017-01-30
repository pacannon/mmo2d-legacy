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

        public IEnumerable<EntityStateUpdate> GenerateUpdates(TimeSpan delta, List<Entity> entities)
        {
            var updates = new List<EntityStateUpdate>();

            var nextLocation = NextLocation(delta, entities);

            if (nextLocation != null)
            {
                var targetEntity = TargetEntity(entities);

                if (targetEntity.Overlapping(nextLocation.Value))
                {
                    updates.Add(new EntityStateUpdate(targetEntity.Id) { Died = true, });
                    updates.Add(new EntityStateUpdate(Id) { RemoveFireball = true, });
                    updates.Add(new EntityStateUpdate(LauncherId) { KillsDelta = 1, });
                }
            }

            else
            {
                updates.Add(new EntityStateUpdate(Id) { RemoveFireball = true, });
            }

            return updates;
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

                var newLocation = Location + Vector2.Multiply(direction, 0.01f) + Vector2.Multiply(direction, t_squared);

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
