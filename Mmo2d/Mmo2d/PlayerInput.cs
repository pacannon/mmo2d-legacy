using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class PlayerInput
    {
        public ButtonState MoveUp { get; set; }
        public ButtonState MoveDown { get; set; }
        public ButtonState MoveLeft { get; set; }
        public ButtonState MoveRight { get; set; }
        public ButtonState Attack { get; set; }

        public void Update(TimeSpan delta)
        {

        }
    }

    public struct ButtonState
    {
        public bool IsPressed { get; set; }
        public TimeSpan? ElapsedTimePressed { get; set; }
    }
}
