using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting a specific type of <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    public interface IAlertTypedPerception : IPerception
    {
    }
}
