using System;
using System.Collections.Generic;

using XRL.World;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting a specific type of <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    public interface IAlertTypedPerception : IPerception
    {
        public Type AlertType => GetAlertType();

        public Type GetAlertType();

        public bool SameAlertAs(IAlertTypedPerception Other)
            => AlertType == Other.AlertType;

        public static bool IsPerceptionOfAlert<A>(IPerception IPerception)
            where A : class, IAlert, new()
            => IPerception is IAlertTypedPerception<A, IPurview<A>>;
    }
}
