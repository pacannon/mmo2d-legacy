﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using Mmo2d.ServerUpdatePackets;
using Newtonsoft.Json;

namespace Mmo2d
{
    public class Entity
    {
        public static readonly Entity Sword = null;// new Entity { width = SwordLength * 2.0f, height = SwordLength * 2.0f, OverriddenColor = Color.Red };

        public Vector2 Location { get; set; }

        //object size
        public float width = 0.2f;
        public float height = 0.2f;

        public long Id { get; set; }

        public Color? OverriddenColor { get; set; }

        public TimeSpan? TimeSinceAttack { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }

        public const float SwordLength = 0.4f;
        public static readonly Color GoblinColor = Color.Green;
        public static readonly TimeSpan SwingSwordAnimationDuration = TimeSpan.FromMilliseconds(100);

        public int Hits { get; set; } = 0;

        public Entity EquippedSword { get; set; }

        public Entity()
        {
        }

        public Entity(Vector2 location) : this()
        {
            Location = location;
        }

        public void Render()
        {
            if (EquippedSword != null && TimeSinceAttack != null && TimeSinceAttack.Value < SwingSwordAnimationDuration)
            {
                EquippedSword.Render();
            }

            //renders a tringle according to the position of the top left vertex of triangle
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(OverriddenColor ?? Color.White);
            GL.Vertex2(TopLeftCorner);

            GL.Color3(OverriddenColor ?? Color.Blue);
            GL.Vertex2(BottomLeftCorner);

            GL.Color3(OverriddenColor ?? Color.Gray);
            GL.Vertex2(BottomRightCorner);

            GL.Color3(OverriddenColor ?? Color.Orange);
            GL.Vertex2(TopRightCorner);

            GL.End();
        }

        public void InputHandler(ServerUpdatePacket message)
        {
            if (message.TypedCharacter == 'w')
            {
                Location = Vector2.Add(Location, new Vector2(0.0f, 0.1f));
            }
            else if (message.TypedCharacter == 's')
            {
                Location = Vector2.Add(Location, new Vector2(0.0f, -0.1f));
            }
            else if (message.TypedCharacter == 'd')
            {
                Location = Vector2.Add(Location, new Vector2(0.1f, 0.0f));
            }
            else if (message.TypedCharacter == 'a')
            {
                Location = Vector2.Add(Location, new Vector2(-0.1f, 0.0f));
            }
            else if (message.TypedCharacter == ' ' && EquippedSword != null)
            {
                TimeSinceAttack = TimeSpan.Zero;
            }
        }

        public void Update(TimeSpan delta, List<Entity> entities)
        {
            if (TimeSinceAttack != null)
            {
                if (TimeSinceAttack == TimeSpan.Zero)
                {
                    foreach (var attackedEntity in entities.Where(e => Attacking(e)))
                    {
                        attackedEntity.Hits++;

                        if (attackedEntity.Hits >= 4)
                        {
                            attackedEntity.TimeSinceDeath = TimeSpan.Zero;
                        }
                    }
                }

                TimeSinceAttack += delta;

                if (TimeSinceAttack > TimeSpan.FromMilliseconds(1000))
                {
                    TimeSinceAttack = null;
                }
            }

            if (TimeSinceDeath != null)
            {
                TimeSinceDeath += delta;
            }

            if (EquippedSword != null)
            {
                EquippedSword.Location = Location;
            }
        }

        public bool Attacking(Entity entity)
        {
            return entity.OverriddenColor == GoblinColor && EquippedSword != null && EquippedSword.Overlapping(entity) && TimeSinceAttack == TimeSpan.Zero;
        }

        public bool Overlapping(Entity entity)
        {
            return Overlapping(entity.TopLeftCorner) || Overlapping(entity.TopRightCorner) ||
                Overlapping(entity.BottomRightCorner) || Overlapping(entity.BottomLeftCorner);
        }

        public bool Overlapping(Vector2 location)
        {
            return TopEdge > location.Y && BottomEdge < location.Y && LeftEdge < location.X && RightEdge > location.X;
        }

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
        

        public void ToJsonString(JsonTextWriter writer)
        {
            writer.WriteStartObject();

            writer.WritePropertyName("width");
            writer.WriteValue(width);
            writer.WritePropertyName("height");
            writer.WriteValue(height);
            writer.WritePropertyName("Location");
            writer.WriteStartObject();
            writer.WritePropertyName("X");
            writer.WriteValue(Location.X);
            writer.WritePropertyName("Y");
            writer.WriteValue(Location.Y);
            writer.WriteEndObject();
            writer.WritePropertyName("Id");
            writer.WriteValue(Id);
            writer.WritePropertyName("Hits");
            writer.WriteValue(Hits);

            writer.WriteEndObject();

        }
    }
}
