using Mmo2d.ServerUpdatePackets;
using System.Collections.Generic;

namespace Mmo2d.State.CharacterController
{
    public class CharacterController : IStateful<CharacterController, IStateDifference<CharacterController>, CharacterController>
    {
        public bool MovingUp { get; set; }
        public bool MovingDown { get; set; }
        public bool MovingLeft { get; set; }
        public bool MovingRight { get; set; }
        public bool FireJumpEvent { get; set; }

        public IEnumerable<IStateDifference<CharacterController>> ResolveDifferences(ServerUpdatePacket packet)
        {
            var differences = new List<IStateDifference<CharacterController>>();



            return differences;
        }
        
        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public CharacterController Apply(IEnumerable<IStateDifference<CharacterController>> stateDifferences)
        {
            var nextCharacterController = (CharacterController)this.Clone();

            foreach (var difference in stateDifferences)
            {
                nextCharacterController = difference.Apply(nextCharacterController);
            }

            return nextCharacterController;
        }
        
        public class MovingUpStateDifference : IStateDifference<CharacterController>
        {
            public bool IsRepeat { get; set; }

            public MovingUpStateDifference(bool isRepeat)
            {
                IsRepeat = isRepeat;
            }

            public CharacterController Apply(CharacterController characterController)
            {
                var clone = (CharacterController)characterController.Clone();

                clone.MovingUp = !characterController.MovingUp;

                return clone;
            }
        }

        public class MovingDownStateDifference : IStateDifference<CharacterController>
        {
            public bool IsRepeat { get; set; }

            public MovingDownStateDifference(bool isRepeat)
            {
                IsRepeat = isRepeat;
            }

            public CharacterController Apply(CharacterController characterController)
            {
                var clone = (CharacterController)characterController.Clone();

                clone.MovingDown = !characterController.MovingDown;

                return clone;
            }
        }

        public class MovingLeftStateDifference : IStateDifference<CharacterController>
        {
            public bool IsRepeat { get; set; }

            public MovingLeftStateDifference(bool isRepeat)
            {
                IsRepeat = isRepeat;
            }

            public CharacterController Apply(CharacterController characterController)
            {
                var clone = (CharacterController)characterController.Clone();

                clone.MovingLeft = !characterController.MovingLeft;

                return clone;
            }
        }

        public class MovingRightStateDifference : IStateDifference<CharacterController>
        {
            public bool IsRepeat { get; set; }

            public MovingRightStateDifference(bool isRepeat)
            {
                IsRepeat = isRepeat;
            }

            public CharacterController Apply(CharacterController characterController)
            {
                var clone = (CharacterController)characterController.Clone();

                clone.MovingRight = !characterController.MovingRight;

                return clone;
            }
        }
    }
}
