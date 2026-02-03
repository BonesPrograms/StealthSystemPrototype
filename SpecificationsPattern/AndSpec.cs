using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AndSpec : Specification
    {
        protected Specification SpecX;
        protected Specification SpecY;

        public AndSpec(Specification SpecX, Specification SpecY)
            : base()
        {
            this.SpecX = SpecX;
            this.SpecY = SpecY;
        }

        public override bool Check()
            => SpecX.Check()
            && SpecY.Check();
    }
}
