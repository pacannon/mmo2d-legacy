namespace Mmo2d.State.Entity
{
    public class MovingUpStateDifference : EntityStateDifference, IStateDifference<Mmo2d.Entity>
    {
        public MovingUpStateDifference(long entityId) : base(entityId)
        {
        }

        public override Mmo2d.Entity Apply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.MovingUp = !entity.MovingUp;

            return clone;
        }

        public override Mmo2d.Entity Unapply(Mmo2d.Entity entity)
        {
            return Apply(entity);
        }
    }

    public class MovingDownStateDifference : EntityStateDifference, IStateDifference<Mmo2d.Entity>
    {
        public MovingDownStateDifference(long entityId) : base(entityId)
        {
        }

        public override Mmo2d.Entity Apply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.MovingDown = !entity.MovingDown;

            return clone;
        }

        public override Mmo2d.Entity Unapply(Mmo2d.Entity entity)
        {
            return Apply(entity);
        }
    }

    public class MovingLeftStateDifference : EntityStateDifference, IStateDifference<Mmo2d.Entity>
    {
        public MovingLeftStateDifference(long entityId) : base(entityId)
        {
        }

        public override Mmo2d.Entity Apply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.MovingLeft = !entity.MovingLeft;

            return clone;
        }

        public override Mmo2d.Entity Unapply(Mmo2d.Entity entity)
        {
            return Apply(entity);
        }
    }

    public class MovingRightStateDifference : EntityStateDifference, IStateDifference<Mmo2d.Entity>
    {
        public MovingRightStateDifference(long entityId) : base(entityId)
        {
        }

        public override Mmo2d.Entity Apply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.MovingRight = !entity.MovingRight;

            return clone;
        }

        public override Mmo2d.Entity Unapply(Mmo2d.Entity entity)
        {
            return Apply(entity);
        }
    }
}
