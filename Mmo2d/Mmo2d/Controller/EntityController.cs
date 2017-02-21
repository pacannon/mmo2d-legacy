using Mmo2d.UserCommands;
using OpenTK;
using System;
using System.Collections.Generic;

namespace Mmo2d.Controller
{
    public class EntityController
    {
        public Dictionary<States, State> ToggleableStates { get; set; }
        
        public Vector2 DirectionOfMotion
        {
            get
            {
                var velocity = Vector2.Zero;

                if (this[States.MoveRight].BoolVal.GetValueOrDefault() && !this[States.MoveLeft].Changed.GetValueOrDefault())
                {
                    velocity += Vector2.UnitX;
                }

                if (this[States.MoveLeft].BoolVal.GetValueOrDefault() && !this[States.MoveRight].Changed.GetValueOrDefault())
                {
                    velocity -= Vector2.UnitX;
                }

                if (this[States.MoveUp].BoolVal.GetValueOrDefault() && !this[States.MoveDown].Changed.GetValueOrDefault())
                {
                    velocity += Vector2.UnitY;
                }

                if (this[States.MoveDown].BoolVal.GetValueOrDefault() && !this[States.MoveUp].Changed.GetValueOrDefault())
                {
                    velocity -= Vector2.UnitY;
                }

                return velocity;
            }
        }

        public EntityController()
        {
            ToggleableStates = new Dictionary<States, State>();

            foreach (States entityControllerToggleableState in Enum.GetValues(typeof(States)))
            {
                ToggleableStates.Add(entityControllerToggleableState, new State(entityControllerToggleableState));
            }
        }

        public State this[States key]
        {
            get
            {
                return ToggleableStates[key];
            }
        }


        public State ApplyUserCommand(UserCommand userCommand)
        {
            var keyEventArgs = userCommand.KeyEventArgs;

            if (keyEventArgs != null)
            {
                switch (keyEventArgs.Key)
                {
                    case OpenTK.Input.Key.W:
                        return new State(States.MoveUp) { BoolVal = keyEventArgs.KeyDown, };

                    case OpenTK.Input.Key.S:
                        return new State(States.MoveDown) { BoolVal = keyEventArgs.KeyDown, };

                    case OpenTK.Input.Key.A:
                        return new State(States.MoveLeft) { BoolVal = keyEventArgs.KeyDown, };

                    case OpenTK.Input.Key.D:
                        return new State(States.MoveRight) { BoolVal = keyEventArgs.KeyDown, };

                    case OpenTK.Input.Key.Space:
                        return new State(States.Jump) { BoolVal = keyEventArgs.KeyDown, };

                    case OpenTK.Input.Key.Number1:
                        return new State(States.CastFireball) { BoolVal = keyEventArgs.KeyDown, };

                    case OpenTK.Input.Key.Number2:
                        return new State(States.CastFrostbolt) { BoolVal = keyEventArgs.KeyDown, };

                    default:
                        return null;
                }
            }

            if (userCommand.DeselectTarget.HasValue)
            {
                return new State(States.TargetId) { LongVal = null, };
            }

            if (userCommand.SetTargetId.HasValue)
            {
                return new State(States.TargetId) { LongVal = userCommand.SetTargetId.Value, };
            }

            if (userCommand.CastFireball.HasValue)
            {
                return new State(States.CastFireball) { BoolVal = true, };
            }

            return null;
        }


        public void ChangeState(State toggeableState)
        {
            this[toggeableState.StateKind].BoolVal = toggeableState.BoolVal;
            this[toggeableState.StateKind].LongVal = toggeableState.LongVal;
            this[toggeableState.StateKind].Changed = toggeableState.Changed;
        }

        public virtual void Update()
        {
            foreach (var state in ToggleableStates.Values)
            {
                state.EraseMemory();
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
            TargetId,
        }

        public class State
        {
            public States StateKind { get; set; }

            public bool? boolVal;
            public long? longVal;

            public bool? BoolVal
            {
                get { return boolVal; }
                set
                {
                    if (boolVal == value)
                    {
                        return;
                    }

                    boolVal = value;
                    
                    Changed = true;
                    ToggledOn = boolVal == true;
                }
            }

            public long? LongVal
            {
                get { return longVal; }

                set
                {
                    if (longVal == value)
                    {
                        return;
                    }

                    longVal = value;

                    Changed = true;
                }
            }

            private bool? changed;

            public State(States entityControllerToggleableState)
            {
                this.StateKind = entityControllerToggleableState;
            }

            public bool? Changed
            {
                get { return changed; }

                set
                {
                    if (changed == value)
                    {
                        return;
                    }

                    changed = value;

                    ToggledOn |= BoolVal.GetValueOrDefault();
                }
            }

            public bool ToggledOn { get; set; }

            public void EraseMemory()
            {
                Changed = null;
                ToggledOn = false;
            }
        }
    }
}
