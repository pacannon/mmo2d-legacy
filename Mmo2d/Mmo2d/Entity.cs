using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using Mmo2d.ServerUpdatePackets;
using Newtonsoft.Json;

namespace Mmo2d
{
    public class Entity
    {
        public static int CharacterTextureId { get; set; }

        public const float Speed = 0.01f;
        public const float width = 0.2f;
        public const float height = 0.2f;
        public const float sprite_size = 17.0f;
        public const float SwordLength = 0.4f;
        public const float HalfAcceration = -2.81f / 2.0f;

        public static readonly Color GoblinColor = Color.Green;
        public static readonly TimeSpan SwingSwordAnimationDuration = TimeSpan.FromMilliseconds(100.0);
        public static readonly TimeSpan JumpAnimationDuration = TimeSpan.FromMilliseconds(400.0);
        public static readonly float JumpVelocity = (float)-JumpAnimationDuration.TotalSeconds * HalfAcceration;

        public long Id { get; set; }        
        public Vector2 Location { get; set; }
        public Color? OverriddenColor { get; set; }
        public TimeSpan? TimeSinceAttack { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }
        public TimeSpan? TimeSinceJump { get; set; }
        public int Kills { get; set; }
        public bool SwordEquipped { get; set; }

        [JsonIgnore]
        public int Hits { get; set; }

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
            GL.BindTexture(TextureTarget.Texture2D, CharacterTextureId);

            RenderSprite(Row, Column);

            if (SwordEquipped && TimeSinceAttack != null)
            {
                RenderSprite(6, 43);
            }
        }

        private void RenderSprite(int row, int column)
        {
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            GL.Begin(PrimitiveType.Quads);

            GL.Color3(Color.Transparent);

            GL.TexCoord2((sprite_size * column) / 917.0f, (1.0f + sprite_size * row) / 203.0f); GL.Vertex2(TopLeftCorner);

            GL.TexCoord2((sprite_size * column) / 917.0f, (17.0f + sprite_size * row) / 203.0f); GL.Vertex2(BottomLeftCorner);

            GL.TexCoord2((16.0f + sprite_size * column) / 917.0f, (17.0f + sprite_size * row) / 203.0f); GL.Vertex2(BottomRightCorner);

            GL.TexCoord2((16.0f + sprite_size * column) / 917.0f, (1.0f + sprite_size * row) / 203.0f); GL.Vertex2(TopRightCorner);

            GL.End();

            GL.Disable(EnableCap.Blend);
        }

        public void InputHandler(ServerUpdatePacket serverUpdatePacket)
        {
            if (serverUpdatePacket.KeyEventArgs != null)
            {
                HandleKeyEventArgs(serverUpdatePacket.KeyEventArgs);
            }

            if (serverUpdatePacket.MousePressed != null)
            {
                UnstagedChanges.Add(() => { AttackKeyDown = serverUpdatePacket.MousePressed.Value; });
            }
        }

        private void HandleKeyEventArgs(KeyEventArgs keyEventArgs)
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
            else if (keyEventArgs.Key != null && keyEventArgs.Key == Key.Space)
            {
                if (keyEventArgs.IsRepeat == false && TimeSinceJump == null)
                {
                    UnstagedChanges.Add(() => { TimeSinceJump = (keyEventArgs.KeyDown ? TimeSpan.Zero : (TimeSpan?)null); });
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

        public void Update(TimeSpan delta, IEnumerable<Entity> entities)
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

                        if (attackedEntity.Hits >= 1)
                        {
                            attackedEntity.TimeSinceDeath = TimeSpan.Zero;
                            Kills++;
                        }
                    }
                }

                TimeSinceAttack += delta;

                if (TimeSinceAttack > SwingSwordAnimationDuration)
                {
                    TimeSinceAttack = null;
                }
            }

            if (TimeSinceJump != null)
            {
                TimeSinceJump += delta;

                if (TimeSinceJump > JumpAnimationDuration)
                {
                    TimeSinceJump = null;
                }
            }

            if (TimeSinceDeath != null)
            {
                TimeSinceDeath += delta;
            }

            if (AttackKeyDown && SwordEquipped)
            {
                TimeSinceAttack = TimeSpan.Zero;
            }

            IncrementPosition();
        }

        public void IncrementPosition()
        {
            var displacementVector = Vector2.Zero;

            if (MoveUpKeyDown)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(Vector2.UnitY, 0.644f));
            }

            if (MoveDownKeyDown)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(-Vector2.UnitY, 0.644f));
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
        }

        public bool Attacking(Entity entity)
        {
            return entity.OverriddenColor == GoblinColor && SwordEquipped && Overlapping(entity) && TimeSinceAttack == TimeSpan.Zero;
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
        public float LeftEdge { get { return Location.X - Width / 2.0f; } }
        [JsonIgnore]
        public float RightEdge { get { return Location.X + Width / 2.0f; } }
        [JsonIgnore]
        public float TopEdge { get { return (Location.Y + height / 2.0f) + Height; } }
        [JsonIgnore]
        public float BottomEdge { get { return (Location.Y - height / 2.0f) + Height; } }
        [JsonIgnore]
        public Vector2 TopLeftCorner { get { return new Vector2(LeftEdge, TopEdge); } }
        [JsonIgnore]
        public Vector2 BottomLeftCorner { get { return new Vector2(LeftEdge, BottomEdge); } }
        [JsonIgnore]
        public Vector2 BottomRightCorner { get { return new Vector2(RightEdge, BottomEdge); } }
        [JsonIgnore]
        public Vector2 TopRightCorner { get { return new Vector2(RightEdge, TopEdge); } }

        [JsonIgnore]
        public float Width
        {
            get
            {
                return width;
            }
        }

        [JsonIgnore]
        public int Row
        {
            get
            {
                return OverriddenColor == GoblinColor ? 3 : OverriddenColor == Color.Red ? 6 : ((int)Id) % 6 + 5;
            }
        }

        [JsonIgnore]
        public int Column
        {
            get
            {
                return OverriddenColor == Color.Red ? 43 : 0;
            }
        }

        [JsonIgnore]
        public float Height
        {
            get
            {
                if (TimeSinceJump == null)
                {
                    return 0.0f;
                }

                var time = (float)TimeSinceJump.Value.TotalSeconds;
                var t2 = time * time;

                var distanceTravelled = time * JumpVelocity;

                return distanceTravelled + HalfAcceration * t2;
            }
        }
    }
}
