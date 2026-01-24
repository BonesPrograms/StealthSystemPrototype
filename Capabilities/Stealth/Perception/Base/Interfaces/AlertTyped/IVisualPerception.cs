using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="Visual"/> <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    public interface IVisualPerception : IAlertTypedPerception<Visual>
    {
        public LightLevel MinimumLightLevel { get; }

        public bool IsSufficientlyLit(Cell Cell)
            => Validate()
            && Cell != null
            && Cell.GetLight() >= MinimumLightLevel;
    }
}
