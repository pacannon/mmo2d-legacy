namespace Mmo2d.State
{
    public interface IStateDifference<T>
    {
        T Apply(T stateDifference);
    }
}
