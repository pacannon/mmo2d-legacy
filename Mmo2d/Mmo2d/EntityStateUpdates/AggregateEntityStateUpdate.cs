using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Mmo2d.EntityStateUpdates
{
    public class AggregateEntityStateUpdate : Dictionary<long, EntityStateUpdate>
    {
        public AggregateEntityStateUpdate() : base()
        {
        }

        public new EntityStateUpdate this[long entityId]
        {
            get
            {
                var update = this.Where(u => u.Key == entityId).Select(u => u.Value).FirstOrDefault();

                if (update == null)
                {
                    update = new EntityStateUpdate(entityId);
                    base[entityId] = update;
                }

                return update;
            }
        }
    }
}
