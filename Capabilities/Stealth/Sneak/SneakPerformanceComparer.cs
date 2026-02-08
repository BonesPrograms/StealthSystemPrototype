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
        protected List<Type> AlertTypes;
        public SneakPerformanceComparer()
            : base()
        {
            AlertTypes = null;
        }
        public SneakPerformanceComparer(IAlert Alert)
            : this()
        {
            AlertTypes = new()
            {
                Alert.Type
            };
        }
        public SneakPerformanceComparer(AlertSet Alerts)
            : this()
        {
            AlertTypes = Alerts.Select(a => a.Type).ToList();
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
