using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Represents the obviousness of a <see cref="IConcealedAction"/> capable of being psychically detected by an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// The degree to which such an activity can be mentally sensed.
    /// </remarks>
    [Serializable]
    public class Psionic : BaseAlert
    {
        public Psionic()
            : base()
        {
        }
        public Psionic(int Intensity)
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
