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
        public BasePerception(GameObject Owner, PerceptionSense Sense, int BaseScore, int BaseRadius)
            : this(Owner)
        {
            this.Sense = Sense;
            this.BaseScore = BaseScore;
            this.BaseRadius = BaseRadius;
        }

        #endregion

        public virtual string ToString(bool Short)
            => (Short ? (GetType()?.Name?[0] ?? '?').ToString() : GetType()?.Name ?? "null?") + "[" + BaseScore + "]@R(" + BaseRadius + ")";

        public override string ToString()
            => ToString(false);

        protected static int RestrainPerceptionScore(int Score, int? Cap = null)
            => Score.ClampWithCap(PERCEPTION_SCORE_CLAMP, Cap);

        protected static int RestrainPerceptionRadius(int Radius, int? Cap = null)
            => Radius.ClampWithCap(PERCEPTION_RADIUS_CLAMP, Cap);

        public abstract bool Validate(GameObject Owner = null);

        protected abstract PerceptionRating? GetPerceptionRating(GameObject Owner = null);

        public abstract int GetScore(GameObject Owner = null);

        public abstract int GetRadius(GameObject Owner = null);

        protected void ClearRating()
            => _Rating = null;

        public virtual int Taper(int Distance)
            => Tapers
                && (Distance - BaseRadius) > 0
            ? BaseScore - (int)Math.Pow(Math.Pow(2.5, Distance - BaseRadius), 1.25)
            : BaseScore;

        public virtual int Roll(GameObject Entity)
        {
            int value = BaseScore;
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
