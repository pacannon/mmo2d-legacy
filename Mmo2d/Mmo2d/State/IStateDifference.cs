namespace Mmo2d.State
{
    public interface IStateDifference
    {
        Mmo2d.Entity Apply(Mmo2d.Entity entity);
        Mmo2d.Entity Unapply(Mmo2d.Entity entity);
    }

    public interface IStateDifference<T> : IStateDifference
    {
        T Apply(T state);
        T Unapply(T state);
    }
}
