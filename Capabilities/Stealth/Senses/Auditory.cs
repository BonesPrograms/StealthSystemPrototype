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

        public Auditory()
            : base(5)
        {
        }
        public Auditory(int Intensity)
            : base(Intensity)
        {
        }
        public Auditory(ISense Source)
            : base(Source)
        {
        }

        public override int GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Auditory(base.Copy());
    }
}
