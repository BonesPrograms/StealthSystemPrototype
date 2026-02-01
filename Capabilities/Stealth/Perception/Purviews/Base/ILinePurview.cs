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
        public bool CheckWithinLine(Cell PerceiverLocation, Cell AlertLocation, out int Distance)
        {
            Distance = -1;
            if (PerceiverLocation is not Cell { InActiveZone: true } origin
                || AlertLocation is not Cell { InActiveZone: true } destination)
                return false;

            Distance = origin.CosmeticDistanceToCell(destination);

            return Distance <= GetEffectiveValue()
                && (!GetOccludes()
                    || origin.HasLOSTo(destination));
        }

        public bool CheckWithinLine(AlertContext Context, out int Distance)
            => CheckWithinLine(
                PerceiverLocation: Context.Perceiver.CurrentCell,
                AlertLocation: Context.AlertLocation,
                Distance: out Distance);
    }
}
