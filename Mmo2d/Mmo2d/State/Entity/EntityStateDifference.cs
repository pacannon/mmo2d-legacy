namespace Mmo2d.State.Entity
{
    public abstract class EntityStateDifference : IStateDifference<Mmo2d.Entity>
    {
        public long EntityId { get; set; }

        protected EntityStateDifference(long entityId)
        {
            EntityId = entityId;
        }

        public abstract Mmo2d.Entity Apply(Mmo2d.Entity state);
    }
}
