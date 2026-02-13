using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Defines methods for determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> producing a <see cref="Cell"/> <see cref="IEnumerable{Cell}"/> through which the determination is made.
    /// </summary>
    public interface IAreaPurview : IPurview
    {
        Cell Origin { get; }

        IEnumerable<Cell> GetCellsInArea()
        {
            if (Origin == null)
                return null;

            if (Origin?.GetAdjacentCells(BaseValue) is not IEnumerable<Cell> cellsInArea)
                return null;

            if (Occludes)
                return cellsInArea;

            return cellsInArea?.Where(c => Origin.HasLOSTo(c));
        }

        bool CheckInArea(ref PerceptionSet.AlertEvent E)
            => GetCellsInArea() is IEnumerable<Cell> areaCells
            && areaCells.Contains(E.AlertLocation);
    }
}
