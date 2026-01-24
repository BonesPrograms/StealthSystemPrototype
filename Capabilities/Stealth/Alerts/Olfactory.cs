using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Represents the obviousness of a <see cref="IConcealedAction"/> capable of being olfactorily detected by an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// The degree to which such an activity emits a smell.
    /// </remarks>
    [Serializable]
    public class Olfactory : BaseAlert
    {
        public Olfactory()
            : base()
        {
        }
        public Olfactory(int Intensity)
            : base(Intensity)
        {
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
        }

        public override void Read(SerializationReader Reader)
        {
        }

        #endregion

        public override BaseAlert AdjustIntensity(int Amount)
            => base.AdjustIntensity(Amount);
    }
}
