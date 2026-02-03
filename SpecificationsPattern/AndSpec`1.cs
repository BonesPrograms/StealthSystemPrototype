using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AndSpec<T> : BinarySpecification<T>
    {
        public AndSpec()
            : base() { }

        public AndSpec(ISpecification<T> SpecX, ISpecification<T> SpecY, T Subject)
            : base(SpecX, SpecY, Subject) { }

        public AndSpec(ISpecification<T> SpecX, ISpecification<T> SpecY)
            : base(SpecX, SpecY, default) { }

        public AndSpec(BinarySpecification<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => SpecX.Check(Subject)
            && SpecY.Check(Subject);

        public static explicit operator AndSpec(AndSpec<T> Operand)
            => (AndSpec)(Operand as ISpecification);
    }
}
