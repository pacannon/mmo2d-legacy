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
using System.Linq.Expressions;

namespace Mmo2d
{
    public class Entity
    {
        public static readonly Entity Sword = new Entity { width = SwordLength * 2.0f, height = SwordLength * 2.0f, OverriddenColor = Color.Red };

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
        public const float Speed = 0.01f;

        [JsonIgnore]
        public List<Action> UnstagedChanges { get; set; }

        public Entity()
        {
            UnstagedChanges = new List<Action>();
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

        public void InputHandler(KeyEventArgs keyEventArgs)
        {
            if (keyEventArgs.Key == Key.W)
            {
                UnstagedChanges.Add(() => { MoveUpKeyDown = keyEventArgs.KeyDown; });
            }
            else if (keyEventArgs.Key == Key.S)
            {
                UnstagedChanges.Add(() => { MoveDownKeyDown = keyEventArgs.KeyDown; });                
            }
            else if (keyEventArgs.Key == Key.D)
            {
                UnstagedChanges.Add(() => { MoveRightKeyDown = keyEventArgs.KeyDown; });                
            }
            else if (keyEventArgs.Key == Key.A)
            {
                UnstagedChanges.Add(() => { MoveLeftKeyDown = keyEventArgs.KeyDown; });                
            }
            else if (keyEventArgs.Key == Key.Space)
            {
                if (!keyEventArgs.IsRepeat)
                {
                    UnstagedChanges.Add(() => { AttackKeyDown = keyEventArgs.KeyDown; });
                }
            }
        }
        
        [JsonIgnore]
        public bool MoveUpKeyDown { get; set; }
        [JsonIgnore]
        public bool MoveDownKeyDown { get; set; }
        [JsonIgnore]
        public bool MoveLeftKeyDown { get; set; }
        [JsonIgnore]
        public bool MoveRightKeyDown { get; set; }
        [JsonIgnore]
        public bool AttackKeyDown { get; set; }

        public void Update(TimeSpan delta, List<Entity> entities)
        {
            UnstagedChanges.ForEach(uc => uc.Invoke());
            UnstagedChanges.Clear();

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

            if (AttackKeyDown && EquippedSword != null)
            {
                TimeSinceAttack = TimeSpan.Zero;
            }

            var displacementVector = Vector2.Zero;

            if (MoveUpKeyDown)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.UnitY);
            }

            if (MoveDownKeyDown)
            {
                displacementVector = Vector2.Add(displacementVector, -Vector2.UnitY);
            }

            if (MoveRightKeyDown)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.UnitX);
            }

            if (MoveLeftKeyDown)
            {
                displacementVector = Vector2.Add(displacementVector, -Vector2.UnitX);
            }

            if (displacementVector != Vector2.Zero)
            {
                displacementVector = Speed * displacementVector.Normalized();

                Location += displacementVector;
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
    }
}
