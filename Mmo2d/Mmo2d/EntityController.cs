using Mmo2d.UserCommands;

namespace Mmo2d
{
    public class EntityController
    {
        //todo: Consider using progressive and past verb tenses
        public bool MoveUp { get; set; }
        public bool MoveUpAtAll { get; set; }

        public bool MoveDown { get; set; }
        public bool MoveDownAtAll { get; set; }

        public bool MoveLeft { get; set; }
        public bool MoveLeftAtAll { get; set; }

        public bool MoveRight { get; set; }
        public bool MoveRightAtAll { get; set; }

        public bool Attack { get; set; }
        public bool AttackAtAll { get; set; }

        public bool Jump { get; set; }
        public bool JumpedAtAll { get; set; }

        public EntityController ApplyUserCommand(UserCommand userCommand)
        {
            var clone = (EntityController)this.MemberwiseClone();

            var keyEventArgs = userCommand.KeyEventArgs;

            if (keyEventArgs != null)
            {
                switch (keyEventArgs.Key)
                {
                    case OpenTK.Input.Key.W:

                        if (keyEventArgs.KeyDown)
                        {
                            clone.MoveUpAtAll = true;
                        }
                        clone.MoveUp = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.S:

                        if (keyEventArgs.KeyDown)
                        {
                            clone.MoveDownAtAll = true;
                        }
                        clone.MoveDown = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.A:

                        if (keyEventArgs.KeyDown)
                        {
                            clone.MoveLeftAtAll = true;
                        }
                        clone.MoveLeft = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.D:

                        if (keyEventArgs.KeyDown)
                        {
                            clone.MoveRightAtAll = true;
                        }
                        clone.MoveRight = keyEventArgs.KeyDown;
                        break;

                    case OpenTK.Input.Key.Space:

                        if (keyEventArgs.KeyDown)
                        {
                            clone.JumpedAtAll = true;
                        }
                        clone.Jump = keyEventArgs.KeyDown;
                        break;
                }
            }

            if (userCommand.MousePressed.HasValue)
            {
                if (userCommand.MousePressed.Value)
                {
                    clone.AttackAtAll = true;
                }

                clone.Attack = userCommand.MousePressed.Value;
            }

            return clone;
        }

        public void Update()
        {
            MoveUpAtAll = false;
            MoveDownAtAll = false;
            MoveLeftAtAll = false;
            MoveRightAtAll = false;
            AttackAtAll = false;
            JumpedAtAll = false;
        }
    }
}
