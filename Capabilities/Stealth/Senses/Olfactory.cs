using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Olfactory : ISense<Olfactory>
    {
        public override int Order => 3;

        public Olfactory()
            : base(5)
        {
        }
        public Olfactory(int Intensity)
            : base(Intensity)
        {
        }
        public Olfactory(ISense Source)
            : base(Source)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Olfactory(base.Copy());
    }
}
