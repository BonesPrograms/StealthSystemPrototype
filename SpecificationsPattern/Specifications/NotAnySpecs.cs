using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotAnySpecs : AnySpecs
    {
        public NotAnySpecs()
            : base() { }

        public NotAnySpecs(IReadOnlyList<Specification> List)
            : base(List) { }

        public NotAnySpecs(Specifications Source)
            : base(Source as IReadOnlyList<Specification>) { }

        public override bool Check()
            => !base.Check();
    }
}
