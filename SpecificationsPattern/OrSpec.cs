using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class OrSpec : BinarySpecification
    {
        public OrSpec(ISpecification SpecX, ISpecification SpecY)
            : base(SpecX, SpecY) { }

        public OrSpec(BinarySpecification Source)
            : base(Source.SpecX, Source.SpecY) { }

        public override bool Check()
            => SpecX.Check()
            || SpecY.Check();
    }
}
