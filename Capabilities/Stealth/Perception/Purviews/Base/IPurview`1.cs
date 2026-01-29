using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Alerts;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/>.
    /// </summary>
    public interface IPurview<A> : IPurview
        where A : IAlert
    {
        public new IAlertTypedPerception ParentPerception { get; set; }

        #region Contracts

        public IPurview<A> SetParentPerception(IAlertTypedPerception ParentPerception);

        public int GetPurviewAdjustment(IAlertTypedPerception ParentPerception, int Value = 0);

        #endregion
    }
}
