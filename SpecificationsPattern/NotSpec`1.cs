using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotSpec<T> : UnarySpecification<T>
    {
        public NotSpec()
            : base() { }

        public NotSpec(ISpecification<T> Spec, T Subject)
            : base(Spec, Subject) { }

        public NotSpec(ISpecification<T> Spec)
            : base(Spec, default) { }

        public NotSpec(UnarySpecification<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => !Spec.Check(Subject);

        public static explicit operator NotSpec(NotSpec<T> Operand)
            => (NotSpec)(Operand as ISpecification);
    }
}
