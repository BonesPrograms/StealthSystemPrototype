using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Alerts;

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
        public static int MIN_VALUE => 0;

        public static int MAX_VALUE => 84;

        public IPerception ParentPerception { get; }

        public int Value { get; }

        public int EffectiveValue { get; }

        /// <summary>
        /// Whether or not this purview is interrupted by <see cref="Render.Occluding"/> <see cref="GameObject"/>s.
        /// </summary>
        public bool Occludes { get; }

        #region Contracts

        /// <summary>
        /// Called once inside the <see cref="IPurview"/>'s default constructor.
        /// </summary>
        /// <remarks>
        /// Override only to make common initialization assignments for derived types.
        /// </remarks>
        public void Construct();

        public IPurview SetParentPerception(IPerception ParentPerception);

        public int GetPurviewAdjustment(IPerception ParentPerception, int Value = 0);

        public int GetEffectiveValue();

        public bool IsWithin(AlertContext Context);

        public void ClearCaches();

        #endregion
        #region Comparioson

        public new int CompareTo(IPurview Other)
            => EitherNull(this, Other, out int comparison)
            ? comparison
            : (Value - Other.Value) +
                (EffectiveValue - Other.EffectiveValue);

        #endregion
    }
}
