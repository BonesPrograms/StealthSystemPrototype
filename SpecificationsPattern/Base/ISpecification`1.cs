using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    /// <summary>
    /// Defines a property that a value type or class implements to refine the determination of whether a specification is met.
    /// </summary>
    public interface ISpecification<T> : ISpecification
    {
        T Subject { get; }

        bool Check(T Subject);
    }
}
