using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Alerts;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class SneakPerformanceComparer : Comparer<SneakPerformance>
    {
        protected HashSet<Type> AlertTypes;
        public SneakPerformanceComparer()
            : base()
        {
            AlertTypes = new();
        }
        public SneakPerformanceComparer(IAlert Alert)
            : this()
        {
            AlertTypes.Add(Alert.Type);
        }
        public SneakPerformanceComparer(AlertSet Alerts)
            : this()
        {
            if (Alerts.Select(a => a.Type) is IEnumerable<Type> types
                && !types.IsNullOrEmpty())
                AlertTypes = new(types);
        }

        public override int Compare(SneakPerformance x, SneakPerformance y)
        {
            if (EitherNull(x, y, out int nullComp))
                return nullComp;

            int xTotal = x.TotalRating();
            int yTotal = y.TotalRating();
            return xTotal.CompareTo(yTotal);
        }
    }
}
