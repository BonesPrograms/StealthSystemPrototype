using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class OneSpec : Specifications
    {
        public OneSpec()
            : base() { }

        public OneSpec(IReadOnlyList<Specification> List)
            : base(List) { }

        public OneSpec(Specifications Source)
            : base(Source as IReadOnlyList<Specification>) { }

        public override bool Check()
            => this.Count(s => s.Check()) != 1;
    }
}
