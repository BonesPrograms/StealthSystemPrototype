using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Psionic : ISense
    {
        public override int Order => 7;

        public Psionic()
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
