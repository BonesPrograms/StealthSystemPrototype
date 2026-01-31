using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;
using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> using a <see cref="BaseDoubleDiffuser"/> or derivative thereof to adjust the effectiveness of the detection over distance.
    /// </summary>
    public interface IDiffusingPurview : IPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        public BaseDoubleDiffuser Diffuser { get; }

        #region Serialization

        #endregion
        #region Contracts

        public void ConfigureDiffuser(Dictionary<string, object> args = null)
        {
            if (!args.IsNullOrEmpty())
            {
                if (args.ContainsKey(nameof(Diffuser.SetSteps))
                    && args[nameof(Diffuser.SetSteps)] is int valueArg)
                {
                    Diffuser.SetSteps(valueArg);
                }
            }
        }

        public double Diffuse(int Value)
        {
            if (Diffuser == null)
                return Value;

            if (Diffuser.TryGetValue(Value, out double diffusedValue))
                return diffusedValue;

            return 0;
        }

        #endregion
    }
}
