using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    public interface IAlert<T> : IAlert
        where T : IAlert<T>, new()
    {
        public new T AdjustIntensity(int Amount);
    }

    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// This serves as a non-generic base which should typically not be derived from directly.
    /// </remarks>
    public interface IAlert
    {
        public int Intensity { get; set; }

        public IAlert AdjustIntensity(int Amount);
    }
}
