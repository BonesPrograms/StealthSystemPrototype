using System;
using System.Collections.Generic;

using XRL.World;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <typeparamref name="A"/> Type <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    /// <typeparam name="A">A class that implements <see cref="IAlert"/> with a default, parameterless constructor.</typeparam>
    public interface IAlertTypedPerception<A> : IPerception
        where A : class, IAlert, new()
    {
        public Type AlertType { get; }

        public V GetTypedPurview<V>()
            where V : BasePurview<A>;

        public new bool CanPerceive(AlertContext Context)
            => CanPerceiveAlert(Context?.ActionAlert)
            && ((IPerception)this).CanPerceive(Context);
    }
}
