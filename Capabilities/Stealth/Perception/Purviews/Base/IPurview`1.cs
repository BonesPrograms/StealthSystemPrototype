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
        public new IAlertTypedPerception<A> ParentPerception { get; set; }

        #region Contracts

        public IPurview<A> SetParentPerception(IAlertTypedPerception<A> ParentPerception);

        public int GetPurviewAdjustment(IAlertTypedPerception<A> ParentPerception, int Value = 0);

        #endregion
    }
}
