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
    public abstract class BasePerception : IComposite, IComparable<BasePerception>
    {
        #region Comparers

        public class RatingComparer : IComparer<BasePerception>
        {
            protected GameObject Entity;

            private RatingComparer()
            {
                Entity = null;
            }
            public RatingComparer(GameObject Entity)
                : base()
            {
                this.Entity = Entity;
            }

            public virtual int Compare(BasePerception x, BasePerception y)
            {
                if (Utils.EitherNull(x, y, out int comparison))
                    return comparison;

                if (Entity!= null)
                {
                    AwarenessLevel awarenessX = x.GetAwareness(Entity, out int rollX);
                    AwarenessLevel awarenessY = y.GetAwareness(Entity, out int rollY);

                    int awarenessComp = awarenessX.CompareTo(awarenessY);
                    if (awarenessComp != 0)
                        return awarenessComp;

                    int rollComp = rollX.CompareTo(rollY);
                    if (rollComp != 0)
                        return rollComp;
                }
                return x.CompareTo(y);
            }
        }
        public class RatingInvertedComparer : RatingComparer
        {
            public RatingInvertedComparer(GameObject Entity)
                : base(Entity)
            {
            }
            public override int Compare(BasePerception x, BasePerception y)
                => base.Compare(y, x);
        }

        #endregion

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

        [NonSerialized]
        private int _BaseScore;
        public int BaseScore
        {
            get => _BaseScore = RestrainPerceptionScore(_BaseScore);
            set => _BaseScore = RestrainPerceptionScore(value);
        }

        [NonSerialized]
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
        protected PerceptionRating? Rating => _Rating ??= GetPerceptionRating();

        public int Score => GetScore();
        public int Radius => GetRadius();

        private int? LastRoll;
        private string LastEntityID;

        [NonSerialized]
        protected bool WantsToClearRating;

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

            LastRoll = null;
            LastEntityID = null;

            WantsToClearRating = false;
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

        protected virtual PerceptionRating? GetPerceptionRating(GameObject Owner = null, int? BaseScore = null, int? BaseRadius = null)
        {
            if (WantsToClearRating)
                ClearRating();

            int baseScore = (BaseScore ?? this.BaseScore) + GetBonusBaseScore();
            int baseRadius = (BaseRadius ?? this.BaseRadius) + GetBonusBaseRadius();

            return new PerceptionRating(baseScore, baseRadius);
        }

        public virtual int GetBonusBaseScore() => 0;
        public virtual int GetBonusBaseRadius() => 0;

        public virtual int GetBonusScore() => 0;
        public virtual int GetBonusRadius() => 0;

        public int GetScore(bool ClearFirst = false)
        {
            if (ClearFirst)
                SetWantsToClearRating();

            int score = GetBonusScore();
            if (Rating is PerceptionRating rating)
                score += rating.Score;

            return score.Clamp(PERCEPTION_SCORE_CLAMP);
        }

        public int GetRadius(bool ClearFirst = false)
        {
            if (ClearFirst)
                SetWantsToClearRating();

            int radius = GetBonusRadius();
            if (Rating is PerceptionRating rating)
                radius += rating.Radius;

            return radius.Clamp(PERCEPTION_RADIUS_CLAMP);
        }

        #endregion

        public virtual string ToString(bool Short, GameObject Entity = null, bool UseLastRoll = false)
        {
            string name = GetType()?.ToStringWithGenerics();

            if (Short)
            {
                name = GetType()?.Name;
                if (name?.IndexOf("`") is int graveIndex
                    && graveIndex >= 0)
                    name = name[..graveIndex].Acronymize() + name[graveIndex..];
                else
                    name = !name.IsNullOrEmpty()
                        ? name.Acronymize()
                        : "?";
            }
            name ??= "null?";
            string rollString = null;
            if (Entity != null)
            {
                AwarenessLevel awareness = GetAwareness(Entity, out int rollValue, UseLastRoll);
                rollString = "(" + awareness.ToString() + ":" + rollValue + ")";
            }
            return name + "[" + BaseScore + ":@R:" + BaseRadius + "]" + rollString;
        }

        public override string ToString()
            => ToString(false);


        public int CompareTo(BasePerception other)
        {
            if (Utils.EitherNull(this, other, out int comparison))
                return comparison;

            int scoreComp = Score.CompareTo(other.Score);
            if (scoreComp != 0)
                return scoreComp;

            return Radius.CompareTo(other.Radius);
        }

        protected static int RestrainPerceptionScore(int Score, int? Cap = null)
            => Score.ClampWithCap(PERCEPTION_SCORE_CLAMP, Cap);

        protected static int RestrainPerceptionRadius(int Radius, int? Cap = null)
            => Radius.ClampWithCap(PERCEPTION_RADIUS_CLAMP, Cap);

        public void SetWantsToClearRating()
            => WantsToClearRating = true;

        protected void ClearRating()
            => _Rating = null;

        public virtual int Taper(int Distance)
            => Tapers
                && Math.Max(0, Distance - Radius) is int outOfRange
                && outOfRange > 0
            ? Score - (int)Math.Pow(Math.Pow(2.5, outOfRange), 1.25)
            : Score;

        public virtual int Roll(GameObject Entity)
        {
            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Entity?.CurrentCell is not Cell { InActiveZone: true } entityCell)
            {
                UnityEngine.Debug.Log(Entity.DebugName + " not in active zone.");
                return 0;
            }

            if (Owner?.CurrentCell is not Cell { InActiveZone: true } myCell)
            {
                UnityEngine.Debug.Log(Owner.DebugName + " not in active zone.");
                return 0;
            }

            if (Occludes
                && !entityCell.HasLOSTo(myCell))
            {
                UnityEngine.Debug.Log(
                    Owner.GetReferenceDisplayName(Stripped: true, Short: true) + 
                    " does not have LOS to " + 
                    Entity.GetReferenceDisplayName(Stripped: true, Short: true));
                return 0;
            }

            int distance = entityCell.CosmeticDistanceto(myCell.Location);
            int score = Taper(distance);

            UnityEngine.Debug.Log(
                nameof(distance) + ": " + distance + " | " +
                nameof(Tapers) + ": " + Tapers + " | " +
                nameof(score) + ": " + score);

            int roll = Stat.RollCached("1d" + score);

            LastRoll = roll;
            LastEntityID = Entity.ID;

            UnityEngine.Debug.Log(nameof(roll) + ": " + roll);

            return roll;
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll, bool UseLastRoll = false)
        {
            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(GetAwareness) + " requires a " + nameof(GameObject) + " to perceive.");

            if (UseLastRoll
                && LastRoll != null
                && LastEntityID == Entity.ID)
                Roll = LastRoll.Value;
            else
                Roll = this.Roll(Entity);

            return (AwarenessLevel)Math.Ceiling(((Roll + 1) / 20.0) - 1);
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, bool UseLastRoll = false)
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

        #region Operator Overloads

        public static bool operator <(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) < 0;

        public static bool operator >(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) > 0;

        public static bool operator <=(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) <= 0;

        public static bool operator >=(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) >= 0;

        #endregion
    }
}
