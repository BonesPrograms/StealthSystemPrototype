using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;
using System.Diagnostics.CodeAnalysis;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Defines methods for determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> producing a <see cref="Cell"/> <see cref="IEnumerable{Cell}"/> through which the determination is made.
    /// </summary>
    public interface ILinePurview : IPurview
    {
        Cell Origin { get; }

        public bool CheckWithinLine(
            Cell AlertLocation,
            out int Distance
            )
        {
            Distance = -1;
            if (Origin is not Cell { InActiveZone: true } origin
                || AlertLocation is not Cell { InActiveZone: true } destination)
                return false;

            Distance = origin.CosmeticDistanceToCell(destination);

            return Distance <= BaseValue
                && (!Occludes
                    || origin.HasLOSTo(destination));
        }

        public bool CheckWithinLine(ref PerceptionSet.AlertEvent E, out int Distance)
            => CheckWithinLine(
                AlertLocation: E.AlertLocation,
                Distance: out Distance);
    }
}
