using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class OrSpec<T> : BinarySpecification<T>
    {
        public OrSpec()
            : base() { }

        public OrSpec(ISpecification<T> SpecX, ISpecification<T> SpecY, T Subject)
            : base(SpecX, SpecY, Subject) { }

        public OrSpec(ISpecification<T> SpecX, ISpecification<T> SpecY)
            : base(SpecX, SpecY, default) { }

        public OrSpec(BinarySpecification<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => SpecX.Check(Subject)
            || SpecY.Check(Subject);

        public static explicit operator OrSpec(OrSpec<T> Operand)
            => (OrSpec)(Operand as ISpecification);
    }
}
