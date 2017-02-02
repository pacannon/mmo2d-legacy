using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using Mmo2d.UserCommands;
using Mmo2d.Controller;
using Newtonsoft.Json;
using Mmo2d.Entities;
using Mmo2d.Textures;

namespace Mmo2d
{
    public class Entity
    {
        public static int CharacterTextureId { get; set; }

        public const float Speed = 0.04f;
        public const float width = 1.0f;
        public const float height = 1.0f;
        public const float HalfAcceration = -9.81f / 2.0f;

        public static readonly TimeSpan SwingSwordAnimationDuration = TimeSpan.FromMilliseconds(500.0);
        public static readonly TimeSpan JumpAnimationDuration = TimeSpan.FromMilliseconds(400.0);
        public static readonly TimeSpan CastFireballCooldown = TimeSpan.FromMilliseconds(200.0);
        public static readonly float JumpVelocity = (float)-JumpAnimationDuration.TotalSeconds * HalfAcceration;

        public long Id { get; set; }

        public bool? IsGoblin { get; set; }
        public bool SwordEquipped { get; set; }

        public Vector2 Location { get; set; }
        public TimeSpan? TimeSinceAttackInitiated { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }
        public TimeSpan? TimeSinceJump { get; set; }
        public TimeSpan? TimeSinceCastFireball { get; set; }

        public long? TargetId { get; set; }
        public long? CastTargetId { get; set; }

        public int Kills { get; set; }

        [JsonIgnore]
        public EntityController EntityController { get; set; }

        [JsonIgnore]
        public int Hits { get; set; }

        public Entity()
        {
            EntityController = new EntityController();
        }

        public void Render(bool selected)
        {
            GL.BindTexture(TextureTarget.Texture2D, CharacterTextureId);

            RenderSprite(Row, Column, selected);
            
            if (SwordEquipped && TimeSinceAttackInitiated != null)
            {
                RenderSprite(6, 43, selected);
            }

            if (IsGoblin.GetValueOrDefault() && RandomFromId() % 100 == 0)
            {
                RenderSprite(RandomFromId() % 4, 3, selected);
                RenderSprite(RandomFromId() % 9, 7 % 9 + 6, selected);
            }

            if (TimeSinceCastFireball != null)
            {
                RenderCastBar(TimeSinceCastFireball.Value, Fireball.CastTime);
            }
        }

        private void RenderCastBar(TimeSpan elapsed, TimeSpan total)
        {
            const float third = (1.0f / 3.0f);
            var percentage = (float)(elapsed.TotalMilliseconds / total.TotalMilliseconds);
            

            SpriteSheet.Ui[25][6].Render(Location - new Vector2(Width, 0.0f), Width, height);
            SpriteSheet.Ui[25][7].Render(Location, Width, height);
            SpriteSheet.Ui[25][8].Render(Location + new Vector2(Width, 0.0f), Width, height);

            SpriteSheet.Ui[27][0].Render(Location - new Vector2(Width, 0.0f), Width, height, percentage / third);

            if (percentage >= third)
            {
                SpriteSheet.Ui[27][1].Render(Location, Width, height, (percentage - third) / third);

                if (percentage >= 2 * third)
                {
                    SpriteSheet.Ui[27][2].Render(Location + new Vector2(Width, 0.0f), Width, height, (percentage - 2 * third) / third);
                }
            }

            GL.End();

            GL.Disable(EnableCap.Blend);
        }

        private void RenderSprite(int row, int column, bool selected)
        {
            SpriteSheet.Characters[row][column].Render(Location + new Vector2(0.0f, Height), Width * (selected ? 1.3f : 1.0f), height * (selected ? 1.3f : 1.0f));
        }

        public void InputHandler(UserCommand userCommand)
        {
            EntityController = EntityController.ApplyUserCommand(userCommand);
        }

        public IEnumerable<EntityStateUpdate> GenerateUpdates(IEnumerable<Entity> entities, Random random)
        {
            var updates = new List<EntityStateUpdate>();
            var generalUpdate = new EntityStateUpdate(Id);

            if (TimeSinceAttackInitiated != null || ((EntityController[EntityController.States.Attack].OnOrToggled) && SwordEquipped))
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

            if (EntityController[EntityController.States.Jump].ToggledOn && TimeSinceJump == null)
            {
                generalUpdate.Jumped = true;
            }

            if (TimeSinceCastFireball == null && CastTargetId != null)
            {
                // Todo: Give Fireball age of timeSince - castTime
                updates.Add(new EntityStateUpdate(Id) { AddFireball = new Fireball(Location, CastTargetId.Value, random.Next(), Id) });

                CastTargetId = null;
            }

            if (EntityController[EntityController.States.CastFireball].ToggledOn && CastTargetId == null && TargetId != null)
            {
                var targets = entities.Where(e => e.IsGoblin.GetValueOrDefault() && e.Id == TargetId);
                Entity target = null;

                if (targets.Count() > 0)
                {
                    target = targets.First();
                }

                if (target != null)
                {
                    updates.Add(new EntityStateUpdate(Id) { StartCastFireball = target.Id, });
                }
            }

            if (EntityController.TargetId != TargetId)
            {
                generalUpdate.SetTargetId = EntityController.TargetId;

                if (generalUpdate.SetTargetId == null)
                {
                    generalUpdate.DeselectTarget = true;
                }
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

            if (EntityController[EntityController.States.MoveUp].OnOrToggled)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(Vector2.UnitY, 0.644f));
            }

            if (EntityController[EntityController.States.MoveDown].OnOrToggled)
            {
                displacementVector = Vector2.Add(displacementVector, Vector2.Multiply(-Vector2.UnitY, 0.644f));
            }

            if (EntityController[EntityController.States.MoveLeft].OnOrToggled)
            {
                displacementVector = Vector2.Add(displacementVector, -Vector2.UnitX);
            }

            if (EntityController[EntityController.States.MoveRight].OnOrToggled)
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

            if (TimeSinceCastFireball != null)
            {
                TimeSinceCastFireball += delta;
                
                if (TimeSinceCastFireball > Fireball.CastTime)
                {
                    TimeSinceCastFireball = null;
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

                if (update.CastFireball != null)
                {
                    TimeSinceCastFireball = TimeSpan.Zero;
                }

                if (update.SetTargetId != null)
                {
                    TargetId = update.SetTargetId;
                }

                if (update.DeselectTarget != null)
                {
                    TargetId = null;
                }

                if (update.StartCastFireball != null)
                {
                    CastTargetId = update.StartCastFireball.Value;
                    TimeSinceCastFireball = TimeSpan.Zero;
                }
            }

            EntityController.Update();
        }

        public bool Attacking(Entity entity)
        {
            return entity.IsGoblin.GetValueOrDefault() && SwordEquipped && Overlapping(entity);
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
                return IsGoblin.GetValueOrDefault() ? 3 : RandomFromId() % 6 + 5;
            }
        }

        [JsonIgnore]
        public int Column
        {
            get
            {
                return RandomFromId() % 2;
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
