using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class BasePerception : IComposite
    {
        [Serializable]
        public struct PerceptionRating : IComposite
        {
            public int Score;
            public int Radius;

            public PerceptionRating(int Score, int Radius)
            {
                this.Score = Score;
                this.Radius = Radius;
            }

            public PerceptionRating(BasePerception BasePerception)
                : this(
                      Score: BasePerception?.BaseScore ?? BASE_PERCEPTION_SCORE,
                      Radius: BasePerception?.BaseRadius ?? BASE_PERCEPTION_RADIUS)
            {
            }
        }

        #region Const & Static Values

        public const int MIN_PERCEPTION_SCORE = 0;
        public const int MAX_PERCEPTION_SCORE = 100;
        public const int BASE_PERCEPTION_SCORE = 20; // AwarenessLevel.Awake

        public const int MIN_PERCEPTION_RADIUS = 0;
        public const int MAX_PERCEPTION_RADIUS = 84; // corner to corner of a single zone.
        public const int BASE_PERCEPTION_RADIUS = 5;

        public static Range PERCEPTION_SCORE_CLAMP => new(MIN_PERCEPTION_SCORE, MAX_PERCEPTION_SCORE);
        public static Range PERCEPTION_RADIUS_CLAMP => new(MIN_PERCEPTION_RADIUS, MAX_PERCEPTION_RADIUS);

        #endregion

        #region Instance PropFields
        public GameObject Owner;

        public PerceptionSense Sense;

        private int _BaseScore;
        public int BaseScore
        {
            get => _BaseScore = RestrainPerceptionScore(_BaseScore);
            set => _BaseScore = RestrainPerceptionScore(value);
        }

        private int _BaseRadius;
        public int BaseRadius
        {
            get => _BaseRadius = RestrainPerceptionScore(_BaseRadius);
            set => _BaseRadius = RestrainPerceptionScore(value);
        }

        public bool Occludes;

        public bool Tapers;

        [NonSerialized]
        private PerceptionRating? _Rating;
        protected PerceptionRating? Rating => _Rating ??= GetPerceptionRating(Owner);

        #endregion

        #region Constructors

        public BasePerception()
        {
            Owner = null;
            Sense = PerceptionSense.None;

            BaseScore = BASE_PERCEPTION_SCORE;
            BaseRadius = BASE_PERCEPTION_RADIUS;

            Occludes = false;
            Tapers = false;

            _Rating = null;
        }
        public BasePerception(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public BasePerception(
            GameObject Owner,
            PerceptionSense Sense,
            int BaseScore,
            int BaseRadius)
            : this(Owner)
        {
            this.Sense = Sense;
            this.BaseScore = BaseScore;
            this.BaseRadius = BaseRadius;
        }
        public BasePerception(
            GameObject Owner,
            PerceptionSense Sense)
            : this(Owner, Sense, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
        {
        }

        #endregion

        #region Abstract Methods

        public abstract bool Validate(GameObject Owner = null);

        protected abstract PerceptionRating? GetPerceptionRating(GameObject Owner = null);

        public abstract int GetScore(GameObject Owner = null, bool ClearFirst = false);

        public abstract int GetRadius(GameObject Owner = null, bool ClearFirst = false);

        #endregion

        public virtual string ToString(bool Short, bool WithRoll = false)
        {
            string name = GetType()?.ToStringWithGenerics();
            if (Short)
            {
                if (name?.IndexOf('`') is int graveIndex
                    && graveIndex >= 0)
                    name = name[..graveIndex].Acronymize() + name[graveIndex..];
                else
                    name = !name.IsNullOrEmpty()
                        ? name[0].ToString()
                        : "?";
            }
            name ??= "null?";
            string rollString = null;
            if (WithRoll)
            {
                AwarenessLevel awareness = GetAwareness(Owner, out int rollValue);
                rollString = "(" + awareness.ToString() + ":" + rollValue + ")";
            }
            return name + "[" + BaseScore + ":@R:" + BaseRadius + "]" + rollString;
        }

        public override string ToString()
            => ToString(false);

        protected static int RestrainPerceptionScore(int Score, int? Cap = null)
            => Score.ClampWithCap(PERCEPTION_SCORE_CLAMP, Cap);

        protected static int RestrainPerceptionRadius(int Radius, int? Cap = null)
            => Radius.ClampWithCap(PERCEPTION_RADIUS_CLAMP, Cap);

        protected void ClearRating()
            => _Rating = null;

        public virtual int Taper(int Distance)
            => Tapers
                && (Distance - GetRadius()) > 0
            ? GetScore() - (int)Math.Pow(Math.Pow(2.5, Distance - GetRadius()), 1.25)
            : GetScore();

        public virtual int Roll(GameObject Entity)
        {
            int value = GetScore();
            if (Entity?.CurrentCell is Cell { InActiveZone: true } entityCell
                && Owner?.CurrentCell is Cell { InActiveZone: true } myCell
                && entityCell.CosmeticDistanceto(myCell.Location) is int distance
                && (!Occludes
                    || entityCell.HasLOSTo(myCell)))
                value = Taper(distance);

            return Stat.RollCached("1d" + value);
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll)
        {
            Roll = this.Roll(Entity ?? Owner);
            return (AwarenessLevel)Math.Ceiling(((Roll + 1) / 20.0) - 1);
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity)
            => GetAwareness(Entity, out _);

        #region Serialization
        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(_BaseScore);
            Writer.WriteOptimized(_BaseRadius);
        }
        public virtual void Read(SerializationReader Reader)
        {
            BaseScore = Reader.ReadOptimizedInt32();
            BaseRadius = Reader.ReadOptimizedInt32();
        }

        #endregion
    }
}
