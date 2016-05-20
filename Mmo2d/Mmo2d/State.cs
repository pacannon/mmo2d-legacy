using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    class State
    {
        public List<Entity> Entities { get; set; }

        public State()
        {
            Entities = new List<Entity>();
        }
    }
}
