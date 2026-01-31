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
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> producing a <see cref="FindPath"/> through which the determination is made.
    /// </summary>
    public interface IPathingPurview : IPurview
    {
        public FindPath LastPath { get; set; }

        public FindPath GetPathTo(AlertContext Context)
            => new(
                StartCell: Context?.Perceiver?.CurrentCell,
                EndCell: Context?.AlertLocation,
                Looker: Context?.Perceiver,
                IgnoreCreatures: true);

        public bool CanPathTo(AlertContext Context)
        {
            if (GetPathTo(Context) is not FindPath findPath
                || !findPath.Found)
                return false;

            List<Cell> steps = findPath.Steps;
            List<int> weights = findPath.Weights;

            int stepsCount = steps.Count;
            int effectiveRangeCents = GetEffectiveValue() * 100;
            for (int i = 0; i < stepsCount; i++)
            {
                if (effectiveRangeCents < 0)
                    return false;

                if (steps[i] == Context.AlertLocation)
                    return true;

                effectiveRangeCents -= weights[i];
            }
            return false;
        }
    }
}
