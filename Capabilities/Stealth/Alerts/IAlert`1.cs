using System;
using System.Collections.Generic;
using System.Text;
using XRL.Collections;

using XRL.World;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    public interface IAlert<A> : IAlert
        where A : IAlert<A>
    {
        public new A AdjustIntensity(int Amount);
        public new A Copy();
    }
}
