namespace Mmo2d.State
{
    public interface IStateful<T>
    {
        T Apply(IStateDifference difference);
        T Unapply(IStateDifference difference);
    }
}
