using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Senses
{
    [Serializable]
    public class Kinesthetic : ISense<Kinesthetic>
    {
        public override int Order => 1;

        public Kinesthetic()
            : base(5)
        {
        }
        public Kinesthetic(int Intensity)
            : base(Intensity)
        {
        }
        public Kinesthetic(ISense Source)
            : base(Source)
        {
        }

        public override int GetIntensity()
            => base.GetIntensity();

        protected override ISense Copy()
            => new Kinesthetic(base.Copy());
    }
}
