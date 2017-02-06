﻿using System;
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
using Mmo2d.EntityStateUpdates;

namespace Mmo2d
{
    public class Entity
    {
        public const float Speed = 0.04f;
        public const float width = 1.0f;
        public const float height = 1.0f;
        public const float HalfAcceration = -9.81f / 2.0f;
        public const float MeleeRange = 1.5f;
        public const int HpRegen = 2;

        public static readonly TimeSpan AutoAttackPeriod = TimeSpan.FromMilliseconds(200.0);
        public static readonly TimeSpan JumpAnimationDuration = TimeSpan.FromMilliseconds(400.0);
        public static readonly TimeSpan CastFireballCooldown = TimeSpan.FromMilliseconds(200.0);
        public static readonly TimeSpan AutoAttackCooldown = TimeSpan.FromMilliseconds(2000.0);
        public static readonly TimeSpan HpRegenTime = TimeSpan.FromMilliseconds(5000);
        public static readonly float JumpVelocity = (float)-JumpAnimationDuration.TotalSeconds * HalfAcceration;

        public long Id { get; set; }

        public bool? IsGoblin { get; set; }

        public Vector2 Location { get; set; }
        public TimeSpan? TimeSinceDeath { get; private set; }
        public TimeSpan? TimeSinceJump { get; set; }
        public TimeSpan? TimeSinceCastFireball { get; set; }
        public TimeSpan? TimeSinceCastFrostbolt { get; set; }        
        public TimeSpan? TimeSinceAutoAttack { get; set; }
        public TimeSpan TimeSinceLastHealthRegen { get; set; }

        public long? TargetId { get; set; }

        public long? CastTargetId { get; set; }

        public int Kills { get; set; }

        [JsonIgnore]
        public EntityController EntityController { get; set; }

        public int Hp { get; set; }
        public int MaximumHp { get { return 10; } }

        public Entity()
        {
            EntityController = new EntityController();
        }

        public void Render(bool selected)
        {
            RenderSprite(Row, Column, selected);
            
            if (TargetId != null)
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

            if (TimeSinceCastFrostbolt != null)
            {
                RenderCastBar(TimeSinceCastFrostbolt.Value, Fireball.CastTime);
            }

            if (selected)
            {
                RenderHp();
            }
        }

        private void RenderCastBar(TimeSpan elapsed, TimeSpan total)
        {
            const float third = (1.0f / 3.0f);
            var percentage = (float)(elapsed.TotalMilliseconds / total.TotalMilliseconds);


            SpriteSheet.Ui[25][6].Render(Location - new Vector2(Width, 0.0f) - Vector2.Multiply(Vector2.UnitY, height), Width, height);
            SpriteSheet.Ui[25][7].Render(Location - Vector2.Multiply(Vector2.UnitY, height), Width, height);
            SpriteSheet.Ui[25][8].Render(Location + new Vector2(Width, 0.0f) - Vector2.Multiply(Vector2.UnitY, height), Width, height);

            SpriteSheet.Ui[27][0].Render(Location - new Vector2(Width, 0.0f) - Vector2.Multiply(Vector2.UnitY, height), Width, height, percentage / third);

            if (percentage >= third)
            {
                SpriteSheet.Ui[27][1].Render(Location - Vector2.Multiply(Vector2.UnitY, height), Width, height, (percentage - third) / third);

                if (percentage >= 2 * third)
                {
                    SpriteSheet.Ui[27][2].Render(Location + new Vector2(Width, 0.0f) - Vector2.Multiply(Vector2.UnitY, height), Width, height, (percentage - 2 * third) / third);
                }
            }

            GL.End();

            GL.Disable(EnableCap.Blend);
        }

        private void RenderHp()
        {
            const float third = (1.0f / 3.0f);
            var percentage = (float)((float)Hp/MaximumHp);

            SpriteSheet.Ui[25][6].Render(Location - new Vector2(Width, 0.0f) + Vector2.Multiply(Vector2.UnitY, height + Height), Width, height);
            SpriteSheet.Ui[25][7].Render(Location + Vector2.Multiply(Vector2.UnitY, height + Height), Width, height);
            SpriteSheet.Ui[25][8].Render(Location + new Vector2(Width, 0.0f) + Vector2.Multiply(Vector2.UnitY, height + Height), Width, height);

            SpriteSheet.Ui[27][6].Render(Location - new Vector2(Width, 0.0f) + Vector2.Multiply(Vector2.UnitY, height + Height), Width, height, percentage / third);

            if (percentage >= third)
            {
                SpriteSheet.Ui[27][7].Render(Location + Vector2.Multiply(Vector2.UnitY, height + Height), Width, height, (percentage - third) / third);

                if (percentage >= 2 * third)
                {
                    SpriteSheet.Ui[27][8].Render(Location + new Vector2(Width, 0.0f) + Vector2.Multiply(Vector2.UnitY, height + Height), Width, height, (percentage - 2 * third) / third);
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

        public void GenerateUpdates(IEnumerable<Entity> entities, AggregateEntityStateUpdate updates, Random random)
        {
            var targetEntity = entities.FirstOrDefault(e => e.Id == TargetId);

            if (EntityController[EntityController.States.Jump].ToggledOn && TimeSinceJump == null)
            {
                updates[Id].Jumped = true;
            }

            if (TimeSinceCastFireball == null && CastTargetId != null)
            {
                // Todo: Give Fireball age of timeSince - castTime
                updates[Id].AddFireball = new Fireball(Location, CastTargetId.Value, random.Next(), Id);

                CastTargetId = null;
            }

            if (TimeSinceCastFrostbolt == null && CastTargetId != null)
            {
                // Todo: Give Fireball age of timeSince - castTime
                updates[Id].AddFireball = new Fireball(Location, CastTargetId.Value, random.Next(), Id);

                CastTargetId = null;
            }

            if ((TimeSinceAutoAttack == null || TimeSinceAutoAttack >= AutoAttackCooldown) && 
                targetEntity != null && IsFoe(targetEntity) && (targetEntity.Location - Location).Length <= MeleeRange)
            {
                updates[Id].AutoAttack = true;
                updates[Id].RemoveFireball = true;

                updates[TargetId.Value].HpDeltas.Add(-2);
                updates[TargetId.Value].SetTargetId = Id;

                if (targetEntity.Hp + updates[targetEntity.Id].HpDeltas.Sum() < 1)
                {
                    updates[targetEntity.Id].Died = true;
                    updates[Id].KillsDelta = (updates[Id].KillsDelta.HasValue ? updates[Id].KillsDelta.Value : 0) + 1;
                }
            }

            if (EntityController[EntityController.States.CastFireball].ToggledOn && CastTargetId == null && TargetId != null)
            {
                var targets = entities.Where(e => (IsFoe(e)) && e.Id == TargetId);
                Entity target = null;

                if (targets.Count() > 0)
                {
                    target = targets.First();
                }

                if (target != null && (target.Location - Location).Length <= Fireball.Range)
                {
                    updates[Id].StartCastFireball = target.Id;
                }
            }

            if (EntityController[EntityController.States.CastFrostbolt].ToggledOn && CastTargetId == null && TargetId != null)
            {
                var targets = entities.Where(e => (IsFoe(e)) && e.Id == TargetId);
                Entity target = null;

                if (targets.Count() > 0)
                {
                    target = targets.First();
                }

                if (target != null && (target.Location - Location).Length <= Fireball.Range)
                {
                    updates[Id].StartCastFrostbolt = target.Id;
                }
            }

            if (EntityController.TargetId != TargetId)
            {
                updates[Id].SetTargetId = EntityController.TargetId;

                if (updates[Id].SetTargetId == null)
                {
                    updates[Id].DeselectTarget = true;
                }
            }

            if (TimeSinceDeath != null)
            {
                updates[Id].Died = true;
            }

            var displacement = IncrementPosition(entities);

            if (displacement != null)
            {
                updates[Id].Displacement = displacement;
            }
        }

        public Vector2? IncrementPosition(IEnumerable<Entity> entities)
        {
            if (IsGoblin.GetValueOrDefault())
            {
                var target = entities.FirstOrDefault(e => e.Id == TargetId);

                if (TargetId != null && target != null)
                {
                    var displacement = target.Location - Location;

                    var keepDistance = MeleeRange * 0.8f;

                    if (displacement.Length > keepDistance)
                    {
                        return Vector2.Multiply(displacement.Normalized(), Math.Min(Speed, keepDistance));
                    }
                }

                return null;
            }

            else
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

            if (TimeSinceCastFireball != null)
            {
                TimeSinceCastFireball += delta;

                if (TimeSinceCastFireball > Fireball.CastTime)
                {
                    TimeSinceCastFireball = null;
                }
            }

            if (TimeSinceCastFrostbolt != null)
            {
                TimeSinceCastFrostbolt += delta;

                if (TimeSinceCastFrostbolt > Fireball.CastTime)
                {
                    TimeSinceCastFrostbolt = null;
                }
            }

            if (TimeSinceAutoAttack != null)
            {
                TimeSinceAutoAttack += delta;

                if (TimeSinceAutoAttack > Fireball.CastTime)
                {
                    TimeSinceAutoAttack = null;
                }
            }

            if (TimeSinceLastHealthRegen != null)
            {
                TimeSinceLastHealthRegen += delta;

                while (TimeSinceLastHealthRegen >= HpRegenTime)
                {
                    TimeSinceLastHealthRegen = TimeSpan.Zero + (TimeSinceLastHealthRegen - HpRegenTime);
                    Hp = Math.Min(Hp + HpRegen, MaximumHp);
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

                if (update.KillsDelta != null)
                {
                    Kills += update.KillsDelta.Value;
                }

                if (update.HpDeltas.Any())
                {
                    Hp = Math.Min(Hp + update.HpDeltas.Sum(), MaximumHp);

                    if (update.HpDeltas.Any(d => d < 0))
                    {
                        TimeSinceLastHealthRegen = TimeSpan.Zero;
                    }
                }

                if (update.SetTargetId != null)
                {
                    TargetId = update.SetTargetId;
                    EntityController.TargetId = TargetId;
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

                if (update.StartCastFrostbolt != null)
                {
                    CastTargetId = update.StartCastFrostbolt.Value;
                    TimeSinceCastFrostbolt = TimeSpan.Zero;
                }

                if (update.AutoAttack != null)
                {
                    TimeSinceAutoAttack = TimeSpan.Zero;
                }
            }

            EntityController.Update();
        }

        public bool Attacking(Entity entity)
        {
            return (IsFoe(entity)) && Overlapping(entity);
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

        public bool IsFoe(Entity potentialFoe)
        {
            return IsGoblin.GetValueOrDefault() != potentialFoe.IsGoblin.GetValueOrDefault();
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
