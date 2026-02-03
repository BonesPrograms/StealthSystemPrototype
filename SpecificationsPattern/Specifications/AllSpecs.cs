using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AllSpecs : Specifications
    {
        public AllSpecs()
            : base () { }

        public AllSpecs(IReadOnlyList<Specification> List)
            : base(List) { }

        public AllSpecs(Specifications Source)
            : base(Source as IReadOnlyList<Specification>) { }

        public override bool Check()
            => this.All(s => s.Check());
    }
}
