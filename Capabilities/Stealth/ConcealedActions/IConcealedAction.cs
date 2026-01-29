using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Contracts a type as having an arbitrary number of <see cref="IAlert"/>s being capable of detection by an <see cref="IPerception"/>, which it contests.
    /// </summary>
    public interface IConcealedAction
        : IComposite
    // Below this is to functionally require that the implementing class inherit from a Rack<IAlert>
        , IList<IAlert>
        , ICollection<IAlert>
        , IEnumerable<IAlert>
        , IReadOnlyList<IAlert>
        , IReadOnlyCollection<IAlert>
    {
        public string ID { get; }

        public string Name { get; }

        public string Action { get; }

        public GameObject Actor { get; }

        public GameObject AlertObject { get; }

        public Cell AlertLocation { get; }

        public SneakPerformance SneakPerformance { get; }

        public bool Aggressive { get; }
    }
}
