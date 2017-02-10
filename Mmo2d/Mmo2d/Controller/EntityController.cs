using Mmo2d.UserCommands;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Mmo2d.Controller
{
    public class EntityController
    {
        public Dictionary<States, ToggleableState> ToggleableStates { get; set; }

        public long? TargetId { get; set; }
        
        public Vector2 DirectionOfMotion
        {
            get
            {
                var velocity = Vector2.Zero;

                if (this[States.MoveRight].On && !this[States.MoveLeft].Toggled)
                {
                    velocity += Vector2.UnitX;
                }

                if (this[States.MoveLeft].On && !this[States.MoveRight].Toggled)
                {
                    velocity -= Vector2.UnitX;
                }

                if (this[States.MoveUp].On && !this[States.MoveDown].Toggled)
                {
                    velocity += Vector2.UnitY;
                }

                if (this[States.MoveDown].On && !this[States.MoveUp].Toggled)
                {
                    velocity -= Vector2.UnitY;
                }

                return velocity;
            }
        }

        public enum States
        {
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            Jump,
            CastFireball,
            CastFrostbolt,
        }

        public EntityController()
        {
            ToggleableStates = new Dictionary<States, ToggleableState>();

            foreach (States entityControllerToggleableStates in Enum.GetValues(typeof(States)))
            {
                ToggleableStates.Add(entityControllerToggleableStates, new ToggleableState());
            }
        }

        public ToggleableState this[States key]
        {
            get
            {
                return ToggleableStates[key];
            }
        }


        public EntityController ApplyUserCommand(UserCommand userCommand)
        {
            var clone = (EntityController)this.MemberwiseClone();

            var keyEventArgs = userCommand.KeyEventArgs;

            if (keyEventArgs != null)
            {
                switch (keyEventArgs.Key)
                {
                    case OpenTK.Input.Key.W:
                        clone[States.MoveUp].On = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.S:
                        clone[States.MoveDown].On = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.A:
                        clone[States.MoveLeft].On = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.D:
                        clone[States.MoveRight].On = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.Space:
                        clone[States.Jump].On = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.E:
                        clone[States.CastFireball].On = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.Q:
                        clone[States.CastFrostbolt].On = keyEventArgs.KeyDown;
                        break;
                }
            }

            if (userCommand.DeselectTarget.HasValue)
            {
                clone.TargetId = null;
            }

            if (userCommand.SetTargetId.HasValue)
            {
                clone.TargetId = userCommand.SetTargetId.Value;
            }

            return clone;
        }

        public virtual void Update()
        {
            foreach (var state in ToggleableStates.Values)
            {
                state.EraseMemory();
            }
        }

        public class ToggleableState
        {
            private bool on;
            private bool toggled;

            public bool On
            {
                get { return on; }

                set
                {
                    if (on == value)
                    {
                        return;
                    }

                    on = value;

                    Toggled = true;
                }
            }

            public bool Toggled
            {
                get { return toggled; }

                set
                {
                    if (toggled == value)
                    {
                        return;
                    }

                    toggled = value;

                    ToggledOn |= On;
                }
            }

            public bool ToggledOn { get; set; }

            public bool OnOrToggled
            {
                get { return On || Toggled; }
            }

            public void Toggle()
            {
                On = !On;
            }

            public void EraseMemory()
            {
                Toggled = false;
                ToggledOn = false;
            }
        }
    }

    public class GoblinEntityController : EntityController
    {
        public Random Random { get; set; }

        public GoblinEntityController(Random random)
        {
            Random = random;
        }

        public override void Update()
        {
            var randomNumber = Random.Next() % 1;

            if (randomNumber == 0)
            {
                randomNumber = Random.Next() % 4;

                switch (randomNumber)
                {
                    case 0:
                        this[States.MoveUp].Toggle();
                        break;
                    case 1:
                        this[States.MoveDown].Toggle();
                        break;
                    case 2:
                        this[States.MoveLeft].Toggle();
                        break;
                    case 3:
                        this[States.MoveRight].Toggle();
                        break;
                }
            }
        }
    }
}
