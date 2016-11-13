using System;
using System.Collections.Generic;

namespace Mmo2d.State
{
    public interface IStateful<T, Y, Z> : ICloneable where Y : IStateDifference<Z>
    {
        T Apply(IEnumerable<Y> difference);
    }
}
