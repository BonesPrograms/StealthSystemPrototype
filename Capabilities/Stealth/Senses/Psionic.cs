using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Psionic : ISense<Psionic>
    {
        public override int Order => 7;

        public Psionic()
            : base(5)
        {
        }
        public Psionic(int Intensity)
            : base(Intensity)
        {
        }
        public Psionic(ISense Source)
            : base(Source)
        {
        }

        public override int GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Psionic(base.Copy());
    }
}
