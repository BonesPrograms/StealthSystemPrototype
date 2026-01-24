using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <typeparamref name="A"/> Type <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    /// <typeparam name="A">A class that implements <see cref="IAlert"/> with a default, parameterless constructor.</typeparam>
    public interface IAlertTypedPerception<A> : IAlertTypedPerception
        where A : class, IAlert, new()
    {
        public new IPurview<A> Purview { get; set; }

        public new IPurview<A> GetPurview();

        public bool CanPerceive<P>(AlertContext<P, A> Context)
            where P : class, IAlertTypedPerception<A>, new()
            => Context?.Alert is IAlert alert
            && CanPerceiveAlert(alert);

        public bool TryPerceive<P>(AlertContext<P, A> Context)
            where P : class, IAlertTypedPerception<A>, new();

        public IDetection<P, A> RaiseDetection<P>(AlertContext<P, A> Context)
            where P : class, IAlertTypedPerception<A>, new();
    }
}
