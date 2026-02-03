using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotSpec : UnarySpecification
    {
        public NotSpec(ISpecification Spec)
            : base(Spec) { }

        public NotSpec(UnarySpecification Source)
            : base(Source.Spec) { }

        public override bool Check()
            => !Spec.Check();
    }
}
