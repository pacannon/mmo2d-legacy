using Mmo2d.UserCommands;
using System;
using System.Collections.Generic;

namespace Mmo2d.Controller
{
    public class EntityController
    {
        public Dictionary<States, ToggleableState> ToggleableStates { get; set; }

        public enum States
        {
            MoveUp,
            MoveDown,
            MoveLeft,
            MoveRight,
            Jump,
            Attack,
            CastFireball,
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

            //set
            //{
            //    Don't see a need for this for now.
            //}
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
                }
            }

            if (userCommand.MousePressed.HasValue)
            {
                clone[States.Attack].On = userCommand.MousePressed.Value;
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
                    ToggledOn = On;
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
