using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mmo2d
{
    public class State
    {
        public List<Entity> Entities { get; set; }

        public State()
        {
            Entities = new List<Entity>();
        }

        public void Render()
        {
            foreach (var entity in Entities)
            {
                entity.Render();
            }
        }

        internal void Update()
        {
            foreach (var entity in Entities)
            {
                entity.Update();
            }
        }
    }
}
