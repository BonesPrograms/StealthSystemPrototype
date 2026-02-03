using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype
{
    [Serializable]
    public class OrSpec : Specification
    {
        protected Specification SpecX;
        protected Specification SpecY;

        public OrSpec(Specification SpecX, Specification SpecY)
            : base()
        {
            this.SpecX = SpecX;
            this.SpecY = SpecY;
        }

        public override bool Check()
            => SpecX.Check()
            || SpecY.Check();
    }
}
