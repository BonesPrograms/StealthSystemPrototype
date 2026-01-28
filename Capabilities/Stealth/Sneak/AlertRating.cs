using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using XRL;
using XRL.World.Parts.Skill;
using XRL.World.Parts.Mutation;
using XRL.Collections;
using XRL.World;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;

using SerializeField = UnityEngine.SerializeField;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class AlertRating : IComposite, IEquatable<AlertRating>, IComparable<AlertRating>
    {
        public static InclusiveRange DefaultClamp => new(-10, 10);

        #region Instance Fields & Properties

        protected IAlert _Alert;
        public virtual IAlert Alert
        {
            get => _Alert;
            protected set => _Alert = value;
        }
        public virtual int Rating
        {
            get => Alert.Intensity.Clamp(Clamp);
            protected set => Alert.Intensity = value.Clamp(Clamp);
        }

        protected InclusiveRange _Clamp;
        public virtual InclusiveRange Clamp
        {
            get => _Clamp;
            protected set => _Clamp = value.Clamp(DefaultClamp);
        }

        #endregion
        #region Constructors

        protected AlertRating()
        {
            Alert = null;
            Clamp = default;
        }
        public AlertRating(IAlert Alert, int Rating, InclusiveRange Clamp)
            : this()
        {
            this.Alert = Alert;
            this.Rating = Rating;
            this.Clamp = Clamp;
        }
        public AlertRating(IAlert Alert)
            : this(Alert, 5, DefaultClamp)
        {
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(Alert);
            Writer.WriteOptimized(Rating);
            Writer.Write(Clamp);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Alert = Reader.ReadComposite() as BaseAlert;
            Rating = Reader.ReadOptimizedInt32();
            Clamp = (InclusiveRange)Reader.ReadComposite();
        }

        #endregion

        public override string ToString()
            => Alert.Name + ":" + Rating + ":[" + Clamp.ToString() + "]";

        public AlertRating SetRating(int Rating)
        {
            this.Rating = Rating;
            return this;
        }

        public AlertRating AdjustRating(int Amount)
            => SetRating(Rating + Amount);

        public AlertRating SetClamp(InclusiveRange Clamp)
        {
            this.Clamp = Clamp.Clamp(DefaultClamp);
            return this;
        }

        public AlertRating SetMin(int Min)
            => SetClamp(new InclusiveRange(Min, Clamp).Clamp(DefaultClamp));

        public AlertRating SetMax(int Max)
            => SetClamp(new InclusiveRange(Clamp, Max).Clamp(DefaultClamp));

        #region Equatability & Comparison

        public bool AlertEquals(AlertRating Other)
        {
            if (EitherNull(this, Other, out bool areEqual))
                return areEqual;

            if (this == Other)
                return true;

            if (Alert != Other.Alert)
                return false;

            return true;
        }

        public bool Equals(AlertRating Other, bool IgnoreClamp)
        {
            if (EitherNull(this, Other, out bool areEqual))
                return areEqual;

            if (this == Other)
                return true;

            if (!AlertEquals(Other))
                return false;

            if (Rating != Other.Rating)
                return false;

            return IgnoreClamp
                || Clamp == Other.Clamp;
        }

        public bool Equals(AlertRating Other)
            => Equals(Other, false);

        public int CompareTo(AlertRating Other)
        {
            if (EitherNull(this, Other, out int nullComp))
                return nullComp;

            if (Equals(Other))
                return 0;

            if (!AlertEquals(Other))
                return 0;

            if (Rating.CompareTo(Other.Rating) is int ratingComp
                && ratingComp != 0)
                return ratingComp;

            return Clamp.CompareTo(Other.Clamp);
        }

        #endregion
        #region Conversions

        public static implicit operator KeyValuePair<string, int>(AlertRating Operand)
            => new(Operand.Alert.Name, Operand.Rating);

        public static explicit operator AlertRating(KeyValuePair<IAlert, int> Operand)
            => new(Operand.Key, Operand.Value, DefaultClamp);

        public static implicit operator KeyValuePair<IAlert, int>(AlertRating Operand)
            => new(Operand.Alert, Operand.Rating);

        #endregion
    }

}
