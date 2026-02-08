using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.AlertExtensions;
using StealthSystemPrototype.Coalescence;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Contracts a type as having an arbitrary number of <see cref="IAlert"/>s being capable of detection by an <see cref="IPerception"/>, which it contests.
    /// </summary>
    public interface IConcealedAction
        : IComposite
    // Below this is to functionally require that the implementing class inherit from a CoalescibleSet<IAlert>
        , IEnumerable<IAlert>
        , ICollection<IAlert>
        , IReadOnlyCollection<IAlert>
        , ISet<IAlert>
        , IList<IAlert>
        , IReadOnlyList<IAlert>
    {
        public string ID { get; }

        public string Name { get; }

        public string Action { get; }

        public GameObject Hider { get; set; }

        public GameObject AlertObject { get; set; }

        public Cell AlertLocation { get; set; }

        public SneakPerformance SneakPerformance => Sneak.GetSneakingEffectFromSource Hider?

        public bool IsAggressive { get; }

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

        public IConcealedAction SetHider(GameObject Hider);

        public IConcealedAction SetAlertObject(GameObject AlertObject = null);

        public IConcealedAction SetAlertLocation(Cell AlertLocation = null);

        public void Configure();
    }
}
