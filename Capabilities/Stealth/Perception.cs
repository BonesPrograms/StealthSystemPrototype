using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.Rules;
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

        private GameObject _Owner;
        public virtual GameObject Owner
        {
            get => _Owner;
            set => _Owner = value;
        }

        private int _Value;
        public virtual int Value
        {
            get => _Value = RestrainPerceptionScore(_Value);
            protected set => _Value = RestrainPerceptionScore(value);
        }

        private int _Radius;
        public virtual int Radius
        {
            get => _Radius = RestrainPerceptionScore(_Radius);
            protected set => _Radius = RestrainPerceptionScore(value);
        }

        public virtual bool Occludes => false;
        public virtual bool Tapers => false;

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

        public virtual int Taper(int Distance)
            => Tapers
                && (Distance - Radius) > 0
            ? Value - (int)Math.Pow(Math.Pow(2.5, Distance - Radius), 1.25)
            : Value;

        public virtual int Roll(GameObject Entity)
        {
            int value = Value;
            if (Entity?.CurrentCell is Cell { InActiveZone: true } entityCell
                && Owner?.CurrentCell is Cell { InActiveZone: true } myCell
                && entityCell.CosmeticDistanceto(myCell.Location) is int distance
                && (!Occludes
                    || entityCell.HasLOSTo(myCell)))
                value = Taper(distance);

            return Stat.RollCached("1d" + value);
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity)
            => (AwarenessLevel)Math.Ceiling(((Roll(Entity) + 1) / 20.0) - 1);

        #region Serialization
        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteGameObject(_Owner);
            Writer.WriteOptimized(_Value);
            Writer.WriteOptimized(_Radius);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Owner = Reader.ReadGameObject();
            Value = Reader.ReadOptimizedInt32();
            Radius = Reader.ReadOptimizedInt32();
        }
        #endregion
    }
}
