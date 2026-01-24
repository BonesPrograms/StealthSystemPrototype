using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Represents the obviousness of a <see cref="BaseConcealedAction"/> capable of being visually detected by an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// The degree to which such an activity is visible.
    /// </remarks>
    [Serializable]
    public class Visual : BaseAlert
    {
        public Visual()
            : base()
        {
        }
        public Visual(int Intensity)
            : base(Intensity)
        {
        }
        public Visual(int Intensity, Dictionary<string, string> Properties)
            : base(Intensity, Properties)
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
