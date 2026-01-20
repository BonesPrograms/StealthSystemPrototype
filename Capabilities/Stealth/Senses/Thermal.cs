using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Thermal : ISense
    {
        public override int Order => 2;

        public Thermal()
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
