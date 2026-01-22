using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Sixth : ISense<Sixth>
    {
        public override int Order => 6;

        public Sixth()
            : base(5)
        {
        }
        public Sixth(int Intensity)
            : base(Intensity)
        {
        }
        public Sixth(ISense Source)
            : base(Source)
        {
        }

        public override int GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Sixth(base.Copy());
    }
}
