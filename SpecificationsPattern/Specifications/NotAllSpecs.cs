using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotAllSpecs : AllSpecs
    {
        public NotAllSpecs()
            : base () { }

        public NotAllSpecs(IReadOnlyList<Specification> List)
            : base(List) { }

        public NotAllSpecs(Specifications Source)
            : base(Source as IReadOnlyList<Specification>) { }

        public override bool Check()
            => !base.Check();
    }
}
