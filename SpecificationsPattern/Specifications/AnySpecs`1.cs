using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AnySpecs<T> : Specifications<T>
    {
        public AnySpecs()
        {
        }

        public override bool Check()
            => this.Any(s => s.Check());
    }
}
