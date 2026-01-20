using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Visual : ISense
    {
        public override int Order => 5;

        public Visual()
        {
        }

        public override double GetIntensity()
            => base.GetIntensity();
    }
}
