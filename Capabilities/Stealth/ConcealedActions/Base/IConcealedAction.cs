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
        , IList<BaseAlert>
        , ICollection<BaseAlert>
        , IEnumerable<BaseAlert>
        , IReadOnlyList<BaseAlert>
        , IReadOnlyCollection<BaseAlert>
    {
        public string GetID();

        public string GetName();

        public string GetAction();

        public GameObject GetHider();

        public GameObject GetAlertObject();

        public Cell GetAlertLocation();

        public SneakPerformance GetSneakPerformance();

        public bool GetAggressive();

        public string GetDescription();

        public IConcealedAction Initialize();

        public void Configure();
    }
}
