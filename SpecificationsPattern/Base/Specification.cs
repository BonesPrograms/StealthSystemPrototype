using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class Specification : ISpecification
    {
        public Specification()
        {
        }

        public abstract bool Check();

        public virtual void Dispose()
        {
        }

        public static implicit operator bool(Specification Spec)
            => Spec.Check();
    }
}
