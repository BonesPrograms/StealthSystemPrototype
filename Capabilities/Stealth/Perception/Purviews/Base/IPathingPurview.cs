using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;
using XRL.Collections;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Defines methods for determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> producing a <see cref="FindPath"/> through which the determination is made.
    /// </summary>
    public interface IPathingPurview : IPurview
    {
        Cell Origin { get; }

        FindPath GetPathTo(ref PerceptionSet.AlertEvent E)
            => new(
                StartCell: Origin,
                EndCell: E.AlertLocation,
                Looker: E.Perceiver,
                IgnoreCreatures: true);

        bool CheckCanPathTo(ref PerceptionSet.AlertEvent E, out int Steps)
        {
            Steps = -1;
            if (GetPathTo(ref E) is not FindPath findPath
                || !findPath.Found)
                return false;

            using var steps = ScopeDisposedList<Cell>.GetFromPoolFilledWith(findPath.Steps);
            using var weights = ScopeDisposedList<int>.GetFromPoolFilledWith(findPath.Weights);

            int stepsCount = steps.Count;
            int effectiveRangeCents = BaseValue * 100;
            for (int i = 0; i < stepsCount; i++)
            {
                if (effectiveRangeCents < 0)
                    return false;

                if (steps[i] == E.AlertLocation)
                {
                    Steps = i;
                    return true;
                }
                effectiveRangeCents -= weights[i];
            }
            return false;
        }
    }
}
