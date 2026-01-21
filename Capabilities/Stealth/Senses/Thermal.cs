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

        public Thermal(int Intensity)
            : base(Intensity)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
