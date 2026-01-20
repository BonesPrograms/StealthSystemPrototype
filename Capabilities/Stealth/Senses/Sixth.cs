using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Sixth : ISense
    {
        public override int Order => 6;

        public Sixth()
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
