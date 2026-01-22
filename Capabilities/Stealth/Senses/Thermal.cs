using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Thermal : ISense<Thermal>
    {
        public override int Order => 2;

        public Thermal()
            : base(5)
        {
        }
        public Thermal(int Intensity)
            : base(Intensity)
        {
        }
        public Thermal(ISense Source)
            : base(Source)
        {
        }

        public override int GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Thermal(base.Copy());
    }
}
