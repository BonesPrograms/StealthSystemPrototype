using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class OneSpec<T> : Specifications<T>
    {
        public OneSpec()
            : base() { }

        public OneSpec(T Subject, IReadOnlyList<ISpecification<T>> List)
            : base(Subject, List) { }

        public OneSpec(T Subject)
            : base(Subject) { }

        public OneSpec(IReadOnlyList<ISpecification<T>> List)
            : base(List) { }

        public OneSpec(Specifications<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => this.Count(s => s.Check(Subject)) != 1;

        public static explicit operator OneSpec(OneSpec<T> Operand)
            => (OneSpec)Operand.Select(s => s as ISpecification);
    }
}
