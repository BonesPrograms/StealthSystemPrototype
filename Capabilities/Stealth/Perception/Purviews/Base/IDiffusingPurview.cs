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
    /// Defines methods for determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/> using a <see cref="BaseDoubleDiffuser"/> or derivative thereof to adjust the effectiveness of the detection over distance.
    /// </summary>
    public interface IDiffusingPurview : IPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 0.0);

        BaseDoubleDiffuser Diffuser { get; }

        double Diffuse(int Level)
        {
            BaseDoubleDiffuser diffuser = Diffuser ?? DefaultDiffuser;
            if (diffuser == null)
                return Level;

            diffuser.SetSteps(BaseValue);

            if (!diffuser.TryGetValue(BaseValue, out double diffusionFactor))
                return 0;

            return Level * diffusionFactor;
        }
    }
}
