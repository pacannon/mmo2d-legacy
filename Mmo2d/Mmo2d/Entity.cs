using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Drawing;
using Mmo2d.ServerUpdatePackets;
using Newtonsoft.Json;
using Mmo2d.State;
using Mmo2d.State.Entity;

namespace Mmo2d
{
    public class Entity : IStateful<Entity, IStateDifference<Entity>, Entity>
    {
        public Vector2 Location { get; set; }

        public static int CharacterTextureId { get; set; }

        //object size
        public const float width = 0.2f;
        public const float height = 0.2f;
        public const float sprite_size = 17.0f;

        [JsonIgnore]
        public int Row
        {
            get
            {
                return OverriddenColor == GoblinColor ? 3 : OverriddenColor == Color.Red ? 6 : (int)(((uint)Id) % 6 + 5);
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

        public long Id { get; set; }

        public Color? OverriddenColor { get; set; }

        [JsonIgnore]
        public float Height { get
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

        public TimeSpan? TimeSinceAttack { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }
        public TimeSpan? TimeSinceJump { get; set; }
        
        public static readonly Color GoblinColor = Color.Green;
        public static readonly TimeSpan SwingSwordAnimationDuration = TimeSpan.FromMilliseconds(100.0);
        public static readonly TimeSpan JumpAnimationDuration = TimeSpan.FromMilliseconds(400.0);
        [JsonIgnore]
        public float JumpVelocity { get { return (float)-JumpAnimationDuration.TotalSeconds * HalfAcceration; } }
        public const float HalfAcceration = -2.81f / 2.0f;
        [JsonIgnore]
        public int Hits { get; set; }
        public int Kills { get; set; }
        
        public bool SwordEquipped { get; set; }
        public const float Speed = 0.01f;

        public Entity()
        {
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

        public IEnumerable<EntityStateDifference> InputHandler(ServerUpdatePacket serverUpdatePacket)
        {
            var stateDifferences = new List<EntityStateDifference>();

            stateDifferences.AddRange(HandleKeyEventArgs(serverUpdatePacket?.KeyEventArgs));

            if (serverUpdatePacket?.MousePressed != null)
            {
                stateDifferences.Add(new AttackingStateDifference(Id));
            }

            return stateDifferences;
        }

        private IEnumerable<EntityStateDifference> HandleKeyEventArgs(KeyEventArgs keyEventArgs)
        {
            var stateDifferences = new List<EntityStateDifference>();

            if (keyEventArgs == null || keyEventArgs.IsRepeat)
            {
                return stateDifferences;
            }

            if (keyEventArgs?.Key == Key.W)
            {
                stateDifferences.Add(new MovingUpStateDifference(Id));
            }
            else if (keyEventArgs?.Key == Key.S)
            {
                stateDifferences.Add(new MovingDownStateDifference(Id));
            }
            else if (keyEventArgs?.Key == Key.A)
            {
                stateDifferences.Add(new MovingLeftStateDifference(Id));
            }
            else if (keyEventArgs?.Key == Key.D)
            {
                stateDifferences.Add(new MovingRightStateDifference(Id));
            }
            else if (keyEventArgs?.Key != null && keyEventArgs.Key == Key.Space)
            {
                if (keyEventArgs.IsRepeat == false && TimeSinceJump == null && keyEventArgs.KeyDown)
                {
                    stateDifferences.Add(new TimeSinceJumpStateDifference(TimeSinceJump, TimeSpan.Zero, Id));
                }
            }

            return stateDifferences;
        }

        [JsonIgnore]
        public bool MovingUp { get; set; }
        [JsonIgnore]
        public bool MovingDown { get; set; }
        [JsonIgnore]
        public bool MovingLeft { get; set; }
        [JsonIgnore]
        public bool MovingRight { get; set; }
        [JsonIgnore]
        public bool Attacking { get; set; }

        public void Update(TimeSpan delta, IEnumerable<Entity> entities)
        {
            if (TimeSinceAttack != null)
            {
                if (TimeSinceAttack == TimeSpan.Zero)
                {
                    foreach (var attackedEntity in entities.Where(e => IsAttacking(e)))
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

            if (Attacking && SwordEquipped)
            {
                TimeSinceAttack = TimeSpan.Zero;
            }

            IncrementPosition();
        }

        public void IncrementPosition()
        {
            var displacementVector = Vector2.Zero;

            if (MovingUp)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(Vector2.UnitY, 0.644f));
            }

            if (MovingDown)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(-Vector2.UnitY, 0.644f));
            }

            if (MovingRight)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.UnitX);
            }

            if (MovingLeft)
            {
                displacementVector = Vector2.Add(displacementVector, -Vector2.UnitX);
            }

            if (displacementVector != Vector2.Zero)
            {
                displacementVector = Speed * displacementVector.Normalized();

                Location += displacementVector;
            }
        }

        public bool IsAttacking(Entity entity)
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

        public object Clone()
        {
            return (Entity)MemberwiseClone();
        }

        public Entity Apply(IEnumerable<IStateDifference<Entity>> differences)
        {
            var nextEntity = (Entity)this.Clone();

            foreach (var difference in differences)
            {
                nextEntity = difference.Apply(this);
            }

            return nextEntity;
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
    }
}
