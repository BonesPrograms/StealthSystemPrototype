using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Auditory : ISense
    {
        public override int Order => 4;

        public Auditory()
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
