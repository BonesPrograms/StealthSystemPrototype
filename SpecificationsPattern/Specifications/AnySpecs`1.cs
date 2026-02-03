using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AnySpecs<T> : Specifications<T>
    {
        public AnySpecs()
            : base() { }

        public AnySpecs(T Subject, IReadOnlyList<ISpecification<T>> List)
            : base(Subject, List) { }

        public AnySpecs(T Subject)
            : base(Subject) { }

        public AnySpecs(IReadOnlyList<ISpecification<T>> List)
            : base(List) { }

        public AnySpecs(Specifications<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => this.Any(s => s.Check(Subject));

        public static explicit operator AnySpecs(AnySpecs<T> Operand)
            => (AnySpecs)Operand.Select(s => s as ISpecification);
    }
}
