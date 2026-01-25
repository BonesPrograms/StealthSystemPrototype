using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="BaseConcealedAction"/> occured within proximity of an <see cref="IPerception"/>
    /// </summary>
    public interface IPurview<A> : IPurview
        where A : class, IAlert, new()
    {
        public new IAlertTypedPerception<A, IPurview<A>> ParentPerception { get; set; }

        #region Contracts

        public void FromReader(SerializationReader Reader, IAlertTypedPerception<A, IPurview<A>> ParentPerception);

        public IPurview<A> SetParentPerception(IAlertTypedPerception<A, IPurview<A>> ParentPerception);

        public int GetPurviewAdjustment(IAlertTypedPerception<A, IPurview<A>> ParentPerception, int Value = 0);

        #endregion
    }
}
