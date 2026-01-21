using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Visual : ISense<Visual>
    {
        public override int Order => 5;

        public Visual()
            : base(5)
        {
        }
        public Visual(int Intensity)
            : base(Intensity)
        {
        }
        public Visual(ISense Source)
            : base(Source)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Visual(base.Copy());
    }
}
