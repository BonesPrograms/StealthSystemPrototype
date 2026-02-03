using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotAllSpecs<T> : AllSpecs<T>
    {
        public NotAllSpecs()
            : base() { }

        public NotAllSpecs(T Subject, IReadOnlyList<ISpecification<T>> List)
            : base(Subject, List) { }

        public NotAllSpecs(T Subject)
            : base(Subject) { }

        public NotAllSpecs(IReadOnlyList<ISpecification<T>> List)
            : base(List) { }

        public NotAllSpecs(Specifications<T> Source)
            : base(Source) { }

        public override bool Check(T Subject)
            => !base.Check();

        public static explicit operator NotAllSpecs(NotAllSpecs<T> Operand)
            => (NotAllSpecs)Operand.Select(s => s as ISpecification);
    }
}
