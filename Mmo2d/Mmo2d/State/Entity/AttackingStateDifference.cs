namespace Mmo2d.State.Entity
{
    public class AttackingStateDifference : EntityStateDifference, IStateDifference<Mmo2d.Entity>
    {
        public AttackingStateDifference(long entityId) : base(entityId)
        {
        }

        public override Mmo2d.Entity Apply(Mmo2d.Entity entity)
        {
            var clone = (Mmo2d.Entity)entity.Clone();

            clone.Attacking = !entity.Attacking;

            return clone;
        }

        public override Mmo2d.Entity Unapply(Mmo2d.Entity entity)
        {
            return Apply(entity);
        }
    }
}
