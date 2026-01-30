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
        where A : IAlert
    {
        public IPurview<A> GetTypedPurview();

        /// <summary>
        /// Writes the <see cref="IPurview{A}"/> member of an <see cref="IAlertTypedPerception{A}"/> during its serialization. 
        /// </summary>
        /// <remarks>
        /// Assume this will be called during the <see cref="IAlertTypedPerception{A}"/>'s call to <see cref="IComposite.Write"/>.
        /// </remarks>
        /// <param name="Writer">The writer that will do the serialization.</param>
        /// <param name="Purview">The <see cref="IAlertTypedPerception{A}.Purview"/> to be written.</param>
        public void WritePurview(SerializationWriter Writer, IPurview<A> Purview);

        /// <summary>
        /// Reads the <see cref="IPurview{A}"/> member of an <see cref="IAlertTypedPerception{A}"/> during its deserialization. 
        /// </summary>
        /// <remarks>
        /// Assume this will be called during the <see cref="IAlertTypedPerception{A}"/>'s call to <see cref="IComposite.Read"/>.
        /// </remarks>
        /// <param name="Reader">The reader that will do the deserialization.</param>
        /// <param name="Purview">An assignable field or variable from which <see cref="IAlertTypedPerception{A}.Purview"/> can be assigned.</param>
        /// <param name="ParentPerception">The <see cref="IAlertTypedPerception{A}"/> whose <see cref="IAlertTypedPerception{A}.Purview"/> is being read, which should be assigned to its <see cref="IPurview{A}.ParentPerception"/> field.</param>
        public void ReadPurview(
            SerializationReader Reader,
            ref IPurview<A> Purview,
            IAlertTypedPerception<A> ParentPerception = null);

        public new Type GetAlertType()
            => typeof(A);

        public new bool CanPerceive(AlertContext Context)
            => CanPerceiveAlert(Context?.Alert)
            && ((IPerception)this).CanPerceive(Context);
    }
}
