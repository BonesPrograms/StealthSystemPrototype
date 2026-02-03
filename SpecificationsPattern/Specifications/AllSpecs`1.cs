using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class AllSpecs<T> : Specifications<T>
    {
        public AllSpecs()
        {
        }

        public override bool Check()
            => this.All(s => s.Check());
    }
}
