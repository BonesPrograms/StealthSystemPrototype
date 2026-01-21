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

        public Olfactory(int Intensity)
            : base(Intensity)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
