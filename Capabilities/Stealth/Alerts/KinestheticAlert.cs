using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Represents the obviousness of a <see cref="IConcealedAction"/> capable of being kinesthetically detected by an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// The degree to which such an activity can be physically felt, by touch.
    /// </remarks>
    [Serializable]
    public struct KinestheticAlert : IAlert, IComposite
    {
        private int _Intensity;

        public int Intensity
        {
            readonly get => _Intensity;
            set => _Intensity = value;
        }

        public KinestheticAlert(int Intensity)
        {
            _Intensity = Intensity;
        }

        #region Serialization

        public readonly void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Intensity);
        }

        public void Read(SerializationReader Reader)
        {
            Intensity = Reader.ReadOptimizedInt32();
        }

        #endregion

        public readonly KinestheticAlert AdjustIntensity(int Amount)
            => new(Intensity + Amount);

        #region Explicit Implementations

        readonly IAlert IAlert.AdjustIntensity(int Amount)
            => AdjustIntensity(Amount);

        #endregion
    }
}
