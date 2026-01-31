using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/>.
    /// </summary>
    public interface IPurview
        : IComposite
        , IComparable<IPurview>
        , IEquatable<IPurview>
    {
        #region Static & Const

        public static int MIN_VALUE => 0;

        public static int MAX_VALUE => 84;

        public static int DEFAULT_VALUE => 4;

        #endregion
        #region Contracts

        #region Field Accessors

        public IPerception GetParentPerception();

        public Type GetAlertType();

        public int GetValue();

        public void SetValue(int Value);

        public int GetEffectiveValue();

        /// <summary>
        /// Gets the boolean value representing whether or not this <see cref="IPurview"/> is interrupted by <see cref="Render.Occluding"/> <see cref="GameObject"/>s.
        /// </summary>
        /// <returns></returns>
        public bool GetOccludes();

        #endregion
        #region Object Life-cycle

        public IPurview SetParentPerception(IPerception ParentPerception);

        public void Configure(Dictionary<string, object> args = null);

        #endregion

        public bool IsForAlert(IAlert Alert);

        public int GetPurviewValueAdjustment(IPerception ParentPerception, int Value = 0);

        public bool IsWithin(AlertContext Context);

        public void ClearCaches();

        #endregion
        #region Comparioson

        public int CompareValueTo(IPurview Other)
            => GetValue() - Other.GetValue();

        public int CompareEffectiveValueTo(IPurview Other)
            => GetEffectiveValue() - Other.GetEffectiveValue();

        public new int CompareTo(IPurview Other)
            => EitherNull(this, Other, out int comparison)
            ? comparison
            : CompareValueTo(Other) + CompareEffectiveValueTo(Other);

        #endregion
    }
}
