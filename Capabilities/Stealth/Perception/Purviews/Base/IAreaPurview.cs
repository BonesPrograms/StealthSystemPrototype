using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Alerts;
using XRL.World.AI.Pathfinding;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> producing a <see cref="Cell"/> <see cref="IEnumerable{Cell}"/> through which the determination is made.
    /// </summary>
    public interface IAreaPurview : IPurview
    {
        public List<Cell> AreaCells { get; set; }

        public IEnumerable<Cell> GetCellsInArea()
        {
            if (ParentPerception?.Owner?.CurrentCell is not Cell { InActiveZone: true } origin
                || origin?.GetAdjacentCells(EffectiveValue) is not IEnumerable<Cell> cellsInArea)
                return null;

            if (!Occludes)
                return cellsInArea;

            return cellsInArea?.Where(c => origin.HasLOSTo(c));
        }

        public bool IsInArea(AlertContext Context)
            => GetCellsInArea() is IEnumerable<Cell> areaCells
            && areaCells.Contains(Context.AlertLocation);
    }
}
