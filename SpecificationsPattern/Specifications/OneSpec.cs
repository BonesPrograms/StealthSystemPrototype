using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public class OneSpec : Specifications
    {
        public OneSpec()
        {
        }

        public override bool Check()
            => this.Count(s => s.Check()) != 1;
    }
}
