using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class Perception : IComposite
    {
        #region Const & Static Values

        public const int MIN_PERCEPTION_VALUE = 0;
        public const int MAX_PERCEPTION_VALUE = 100;
        public const int BASE_PERCEPTION_VALUE = 20; // AwarenessLevel.Awake

        public const int MIN_PERCEPTION_RADIUS = 0;
        public const int MAX_PERCEPTION_RADIUS = 84; // corner to corner of a single zone.
        public const int BASE_PERCEPTION_RADIUS = 5;

        #endregion

        public GameObject Owner;

        private int _Value;
        protected virtual int Value
        {
            get => _Value = RestrainPerceptionScore(_Value);
            set => _Value = RestrainPerceptionScore(value);
        }

        private int _Radius;
        protected virtual int Radius
        {
            get => _Radius = RestrainPerceptionScore(_Radius);
            set => _Radius = RestrainPerceptionScore(value);
        }

        #region Constructors

        public Perception()
        {
            Owner = null;
            Value = 0;
            Radius = 0;
        }
        public Perception(GameObject Owner, int Value, int Radius)
            : this()
        {
            this.Owner = Owner;
            this.Value = Value;
            this.Radius = Radius;
        }

        #endregion

        public virtual string ToString(bool Short)
            => (Short ? (GetType()?.Name?[0] ?? '?').ToString() : GetType()?.Name ?? "null?") + "[" + Value + "]@R(" + Radius + ")";

        public override string ToString()
            => ToString(false);

        protected static int RestrainPerceptionScore(int Value, int? Cap = null)
            => Value.Restrain(MIN_PERCEPTION_VALUE, Math.Max(Cap ?? MAX_PERCEPTION_VALUE, MAX_PERCEPTION_VALUE));

        protected static int RestrainPerceptionRadius(int Radius, int? Cap = null)
            => Radius.Restrain(MIN_PERCEPTION_RADIUS, Math.Max(Cap ?? MAX_PERCEPTION_RADIUS, MAX_PERCEPTION_RADIUS));

        public abstract int RollPerception();

        #region Serialization
        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(_Value);
            Writer.WriteOptimized(_Radius);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Value = Reader.ReadOptimizedInt32();
            Radius = Reader.ReadOptimizedInt32();
        }
        #endregion
    }
}
