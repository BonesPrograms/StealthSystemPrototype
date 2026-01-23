using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using System.Reflection;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of providing the base most functionality required to respond to <see cref="IConcealedAction"/>s and issue <see cref="IAlert"/>s.
    /// </summary>
    public interface IPerception
    {
        public string Name => GetName();

        public string ShortName => GetName(true);

        public GameObject Owner { get; set; }

        public PerceptionRack Rack { get; }

        public IPurview Radius { get; set; }

        public int Level { get; }

        public IEnumerable<Cell> RadiusAreaCells { get; }

        public abstract string GetName(bool Short = false);
    }
}
