using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> producing a <see cref="Cell"/> <see cref="IEnumerable{Cell}"/> through which the determination is made.
    /// </summary>
    public interface ILinePurview : IPurview
    {
        public bool IsWithinLine(AlertContext Context)
            => Context?.Perceiver?.CurrentCell is Cell { InActiveZone: true } origin
            && Context?.AlertLocation is Cell { InActiveZone: true } destination
            && origin.CosmeticDistanceToCell(destination) <= EffectiveValue
            && (!Occludes
                || origin.HasLOSTo(destination));
    }
}
