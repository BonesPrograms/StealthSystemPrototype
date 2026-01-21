using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Auditory : ISense<Auditory>
    {
        public override int Order => 4;

        public Auditory(int Intensity)
            : base(Intensity)
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
