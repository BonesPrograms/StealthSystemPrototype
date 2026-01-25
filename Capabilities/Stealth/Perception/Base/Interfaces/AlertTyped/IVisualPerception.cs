using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="Visual"/> <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/> by way of an <see cref="VisualPurview"/>.
    /// </summary>
    public interface IVisualPerception : IAlertTypedPerception<Visual, VisualPurview>
    {
        public static LightLevel DefaultMinimumLightLevel => LightLevel.Light;

        public LightLevel MinimumLightLevel => DefaultMinimumLightLevel;

        public bool IsSufficientlyLit(Cell Cell)
            => Validate()
            && Cell != null
            && Cell.GetLight() >= MinimumLightLevel;
    }
}
