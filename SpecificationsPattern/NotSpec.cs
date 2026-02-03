using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype
{
    [Serializable]
    public class NotSpec : Specification
    {
        protected Specification Spec;

        public NotSpec(Specification Spec)
            : base()
        {
            this.Spec = Spec;
        }

        public override bool Check()
            => !Spec.Check();
    }
}
