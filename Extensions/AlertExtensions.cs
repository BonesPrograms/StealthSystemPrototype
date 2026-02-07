using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Alerts;
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

        public static A Coalesce<A>(this A Alert, A OtherAlert, CoalesceMethod Method)
            where A : IAlert
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Alert), Alert.ToString()),
                    Debug.Arg(nameof(OtherAlert), OtherAlert.ToString()),
                    Debug.Arg(nameof(Method), Method.ToStringWithNum()),
                });

            GetMinMax(out A lowestAlert, out A highestAlert,
                Alerts: new A[]
                {
                    Alert,
                    OtherAlert,
                });

            A replacementVessel = Method switch
            {
                CoalesceMethod.Lowest => lowestAlert,
                CoalesceMethod.Highest => highestAlert,
                _ => default,
            };
            if (Method != CoalesceMethod.Merge)
            {
                Alert.Intensity = replacementVessel.Intensity;
                Alert.Properties = replacementVessel.Properties;
            }
            else
            {
                Alert.Intensity += OtherAlert.Intensity;

                Alert.Properties ??= new();
                foreach ((string name, string value) in OtherAlert.Properties)
                {
                    if (Alert.Properties.ContainsKey(name))
                    {
                        string propValue = Alert.Properties[name];
                        if (!propValue.IsNullOrEmpty())
                            propValue += ",";

                        Alert.Properties[name] = propValue + value;
                    }
                    else
                        Alert.Properties[name] = value;
                }
            }
            return Alert;
        }
    }
}
