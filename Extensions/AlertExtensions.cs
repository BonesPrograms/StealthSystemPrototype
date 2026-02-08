using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Coalescence;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Const;
using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    public static class AlertExtensions
    {
        #region Helpers

        #endregion

        public static void GetMinMax<A>(out A Min, out A Max, params A[] Alerts)
            where A : IAlert
        {
            Min = default;
            Max = default;

            if (Alerts.IsNullOrEmpty())
                return;
            else
            if (Alerts.Length == 1)
            {
                Min = Alerts[0];
                Max = Alerts[0];
            }
            else
            {
                foreach (A currentAlert in Alerts)
                {
                    if (Math.Min(Min?.Intensity ?? int.MinValue, currentAlert.Intensity) != (Min?.Intensity ?? int.MinValue))
                        Min = currentAlert;
                    if (Math.Max(currentAlert.Intensity, Max?.Intensity ?? int.MaxValue) != (Max?.Intensity ?? int.MaxValue))
                        Max = currentAlert;
                }
            }
        }
    }
}
