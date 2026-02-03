using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AllSpecs<T> : Specifications<T>
    {
        public AllSpecs()
            : base() { }

        public AllSpecs(T Subject, IReadOnlyList<ISpecification<T>> List)
            : base(Subject, List) { }

        public AllSpecs(T Subject)
            : base(Subject) { }

        public AllSpecs(IReadOnlyList<ISpecification<T>> List)
            : base(List) { }

        public AllSpecs(Specifications<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => this.All(s => s.Check(Subject));

        public static explicit operator AllSpecs(AllSpecs<T> Operand)
            => (AllSpecs)Operand.Select(s => s as ISpecification);
    }
}
