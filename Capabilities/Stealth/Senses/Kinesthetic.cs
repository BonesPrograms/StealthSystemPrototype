using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Kinesthetic : ISense
    {
        public override int Order => 1;

        public Kinesthetic()
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
