using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Mmo2d.UserCommands;
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
        public static readonly TimeSpan SwingSwordAnimationDuration = TimeSpan.FromMilliseconds(500.0);
        public static readonly TimeSpan JumpAnimationDuration = TimeSpan.FromMilliseconds(400.0);
        public static readonly float JumpVelocity = (float)-JumpAnimationDuration.TotalSeconds * HalfAcceration;

        public long Id { get; set; }

        public Color? OverriddenColor { get; set; }
        public bool SwordEquipped { get; set; }

        public Vector2 Location { get; set; }
        public TimeSpan? TimeSinceAttackInitiated { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }
        public TimeSpan? TimeSinceJump { get; set; }
        public int Kills { get; set; }

        [JsonIgnore]
        public EntityController EntityController { get; set; }

        [JsonIgnore]
        public int Hits { get; set; }

        public Entity()
        {
            EntityController = new EntityController();
        }

        public void Render()
        {
            GL.BindTexture(TextureTarget.Texture2D, CharacterTextureId);

            RenderSprite(Row, Column);

            if (SwordEquipped && TimeSinceAttackInitiated != null)
            {
                RenderSprite(6, 43);
            }

            if (OverriddenColor == Color.Green && RandomFromId() % 100 == 0)
            {
                RenderSprite(RandomFromId() % 4, 3);
                RenderSprite(RandomFromId() % 9, 7 % 9 + 6);
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

        public void InputHandler(UserCommand userCommand)
        {
            EntityController = EntityController.ApplyUserCommand(userCommand);
        }

        public IEnumerable<EntityStateUpdate> GenerateUpdates(IEnumerable<Entity> entities)
        {
            var updates = new List<EntityStateUpdate>();
            var generalUpdate = new EntityStateUpdate(Id);

            if (TimeSinceAttackInitiated != null || ((EntityController.Attack || EntityController.AttackAtAll) && SwordEquipped))
            {
                if (TimeSinceAttackInitiated == null)
                { 
                    if (TimeSinceAttackInitiated == null && SwordEquipped)
                    {
                        generalUpdate.AttackInitiated = true;
                    }
                }

                foreach (var attackedEntity in entities.Where(e => Attacking(e)))
                {
                    updates.Add(new EntityStateUpdate(attackedEntity.Id) { HitsDelta = 1 });                    
                    updates.Add(new EntityStateUpdate(attackedEntity.Id) { Died = true });

                    if (generalUpdate.KillsDelta == null)
                    {
                        generalUpdate.KillsDelta = 0;
                    }

                    generalUpdate.KillsDelta++;
                }
            }

            if (EntityController.JumpedAtAll && TimeSinceJump == null)
            {
                generalUpdate.Jumped = true;
            }

            if (TimeSinceDeath != null)
            {
                generalUpdate.Died = true;
            }

            generalUpdate.Displacement = IncrementPosition();

            if (generalUpdate.ContainsInformation)
            {
                updates.Add(generalUpdate);
            }

            return updates;
        }

        public Vector2? IncrementPosition()
        {
            var displacementVector = Vector2.Zero;

            if (EntityController.MoveUp || EntityController.MoveUpAtAll)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(Vector2.UnitY, 0.644f));
            }

            if (EntityController.MoveDown || EntityController.MoveDownAtAll)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(-Vector2.UnitY, 0.644f));
            }

            if (EntityController.MoveLeft || EntityController.MoveLeftAtAll)
            {
                displacementVector = Vector2.Add(displacementVector, -Vector2.UnitX);
            }

            if (EntityController.MoveRight || EntityController.MoveRightAtAll)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.UnitX);
            }

            if (displacementVector != Vector2.Zero)
            {
                displacementVector = Speed * displacementVector.Normalized();

                return displacementVector;
            }

            return null;
        }

        public void ApplyUpdates(IEnumerable<EntityStateUpdate> updates, TimeSpan delta)
        {
            if (TimeSinceDeath != null)
            {
                TimeSinceDeath += delta;
            }

            if (TimeSinceJump != null)
            {
                TimeSinceJump += delta;

                if (TimeSinceJump > JumpAnimationDuration)
                {
                    TimeSinceJump = null;
                }
            }

            if (TimeSinceAttackInitiated != null)
            {
                TimeSinceAttackInitiated += delta;

                if (TimeSinceAttackInitiated > SwingSwordAnimationDuration)
                {
                    TimeSinceAttackInitiated = null;
                }
            }

            foreach (var update in updates)
            {
                if (update.Displacement != null)
                {
                    Location += update.Displacement.Value;
                }

                if (update.Died != null)
                {
                    TimeSinceDeath = TimeSpan.Zero;
                }

                if (update.Jumped != null)
                {
                    TimeSinceJump = TimeSpan.Zero;
                }

                if (update.AttackInitiated != null)
                {
                    TimeSinceAttackInitiated = TimeSpan.Zero;
                }

                if (update.KillsDelta != null)
                {
                    Kills += update.KillsDelta.Value;
                }

                if (update.HitsDelta != null)
                {
                    Hits += update.HitsDelta.Value;
                }
            }

            EntityController.Update();
        }

        public bool Attacking(Entity entity)
        {
            return entity.OverriddenColor == GoblinColor && SwordEquipped && Overlapping(entity);
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

        public int RandomFromId()
        {
            var integer = unchecked((int)(Id));
            return integer < 0 ? -integer : integer;
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
                return OverriddenColor == GoblinColor ? 3 : OverriddenColor == Color.Red ? 6 : RandomFromId() % 6 + 5;
            }
        }

        [JsonIgnore]
        public int Column
        {
            get
            {
                return OverriddenColor == Color.Red ? 43 : RandomFromId() % 2;
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
