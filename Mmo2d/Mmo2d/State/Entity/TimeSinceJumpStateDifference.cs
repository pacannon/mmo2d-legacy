using System;

namespace Mmo2d.State.Entity
{
    public class TimeSinceJumpStateDifference : EntityStateDifference, IStateDifference<Mmo2d.Entity>
    {
        public TimeSpan? Before { get; set; }
        public TimeSpan? After { get; set; }

        public TimeSinceJumpStateDifference(TimeSpan? before, TimeSpan? after, long entityId) : base(entityId)
        {
            Before = before;
            After = after;
        }

        public override Mmo2d.Entity Apply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.TimeSinceJump = After;

            return clone;
        }

        public override Mmo2d.Entity Unapply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.TimeSinceJump = Before;

            return clone;
        }
    }
}
