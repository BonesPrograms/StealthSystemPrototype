using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    /// <summary>
    /// Defines a method that a value type or class implements to determinate whether a specification is met.
    /// </summary>
    public interface ISpecification : IComposite, IDisposable
    {
        bool Check();
    }
}
