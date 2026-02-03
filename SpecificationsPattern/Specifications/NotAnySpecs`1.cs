using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotAnySpecs<T> : AnySpecs<T>
    {
        public NotAnySpecs()
            : base() { }

        public NotAnySpecs(T Subject, IReadOnlyList<ISpecification<T>> List)
            : base(Subject, List) { }

        public NotAnySpecs(T Subject)
            : base(Subject) { }

        public NotAnySpecs(IReadOnlyList<ISpecification<T>> List)
            : base(List) { }

        public NotAnySpecs(Specifications<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => !base.Check();

        public static explicit operator NotAnySpecs(NotAnySpecs<T> Operand)
            => (NotAnySpecs)Operand.Select(s => s as ISpecification);
    }
}
