using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;

using StealthSystemPrototype.Events;
using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class BasePerception : IComposite, IComparable<BasePerception>
    {
        #region Helpers

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

        #endregion
        #region Const & Static Values

        public const int MIN_SCORE = 0;
        public const int MAX_SCORE = 100;
        public static ClampedRange BASE_SCORE => new(10..30, SCORE_CLAMP); // ~10 either side of AwarenessLevel.Awake

        public const int MIN_RADIUS = 0;
        public const int MAX_RADIUS = 84; // corner to corner of a single zone.
        public static Radius BASE_RADIUS => new(5);

        public static Range SCORE_CLAMP => new(MIN_SCORE, MAX_SCORE);
        public static Range RADIUS_CLAMP => new(MIN_RADIUS, MAX_RADIUS);

        public static Radius.RadiusFlags VisualFlag => Radius.RadiusFlags.Line | Radius.RadiusFlags.Occludes | Radius.RadiusFlags.Diffuses;
        public static Radius.RadiusFlags AuditoryFlag => Radius.RadiusFlags.Area | Radius.RadiusFlags.Pathing | Radius.RadiusFlags.Diffuses;
        public static Radius.RadiusFlags OlfactoryFlag => Radius.RadiusFlags.Area | Radius.RadiusFlags.Pathing | Radius.RadiusFlags.Diffuses;

        #endregion

        #region Instance PropFields

        public GameObject Owner;

        public PerceptionSense Sense;

        [NonSerialized]
        public ClampedRange BaseScore;

        [NonSerialized]
        public Radius BaseRadius;

        protected ClampedRange _Score;
        public ClampedRange Score => _Score ??= GetScore(this);

        protected Radius _Radius;
        public Radius Radius => _Radius ??= GetRadius(this);

        public Radius.RadiusFlags RadiusFlags => Radius.Flags;
        public bool Occludes => Radius.Occludes();
        public bool Tapers => Radius.Diffuses();

        protected int? LastRoll;
        protected string LastEntityID;

        [NonSerialized]
        protected bool WantsToClearRating;

        #endregion

        #region Constructors

        public BasePerception()
        {
            Owner = null;
            Sense = PerceptionSense.None;

            BaseScore = BASE_SCORE;
            BaseRadius = BASE_RADIUS;

            _Score = null;
            _Radius = null;

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
            ClampedRange BaseScore,
            Radius BaseRadius)
            : this(Owner)
        {
            this.Sense = Sense;
            this.BaseScore = BaseScore;
            this.BaseRadius = BaseRadius;
        }
        public BasePerception(
            GameObject Owner,
            PerceptionSense Sense)
            : this(Owner, Sense, BASE_SCORE, BASE_RADIUS)
        {
        }

        #endregion

        #region Abstract Methods

        public abstract bool Validate(GameObject Owner = null);

        public virtual int GetBonusBaseScore() => 0;
        public virtual int GetBonusBaseRadius() => 0;

        public virtual int GetBonusScore() => 0;
        public virtual int GetBonusRadius() => 0;

        #endregion

        protected ClampedRange GetScore<T>(T Perception = null)
            where T : BasePerception
            => GetPerceptionScoreEvent.GetFor(
                    Perceiver: Owner,
                    Perception: Perception ?? (T)this,
                    BaseScore: BaseScore.AdjustBy(GetBonusBaseScore()))
                ?.AdjustBy(GetBonusScore());

        protected Radius GetRadius<T>(T Perception = null)
            where T : BasePerception
            => GetPerceptionRadiusEvent.GetFor(
                    Perceiver: Owner,
                    Perception: Perception ?? (T)this,
                    BaseRadius: BaseRadius.AdjustBy(GetBonusBaseRadius()))
                ?.AdjustBy(GetBonusRadius());

        public virtual string GetName(bool Short = false)
            => GetType()?.ToStringWithGenerics(Short) ?? (Short ? "?" : "null?");

        public virtual string ToString(bool Short, GameObject Entity = null, bool UseLastRoll = false)
        {
            string rollString = null;
            if (Entity != null)
            {
                AwarenessLevel awareness = GetAwareness(Entity, out int rollValue, UseLastRoll);
                rollString = "(" + awareness.ToString() + ":" + rollValue + ")";
            }
            return GetName(Short) + "[" + BaseScore + ":@R:" + BaseRadius + "]" + rollString;
        }

        public override string ToString()
            => ToString(false);

        public void ClearScore()
            => _Score = null;

        public void ClearRadius()
            => _Radius = null;

        public void ClearRating()
        {
            ClearScore();
            ClearRadius();
        }

        public int CompareTo(BasePerception other)
        {
            if (EitherNull(this, other, out int comparison))
                return comparison;

            RangeComparer rangeComparer = new(Score, other.Score);
            int scoreComp = rangeComparer.CompareAverage() + rangeComparer.CompareBreadth(Invert: true);
            if (scoreComp != 0)
                return scoreComp;

            return Radius.CompareTo(other.Radius);
        }

        public virtual ClampedRange Taper(int Distance)
        {
            if (Math.Max(0, Radius.GetValue() - Distance) is int outOfRange
                && outOfRange > 0)
            {
                if (Tapers)
                    return Score.AdjustBy(-(int)Math.Pow(Math.Pow(2.5, outOfRange), 1.25));
                else
                    return ClampedRange.Empty;
            }
            return Score;
        }

        public virtual int Roll(GameObject Entity, bool UseLastRoll = false)
        {
            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            if (UseLastRoll
                && Entity.ID == LastEntityID
                && LastRoll is int lastRoll)
                return lastRoll;

            if (Entity?.CurrentCell is not Cell { InActiveZone: true } entityCell)
            {
                UnityEngine.Debug.Log(" ".ThisManyTimes(4) + Entity.DebugName + " not in active zone.");
                return 0;
            }

            if (Owner?.CurrentCell is not Cell { InActiveZone: true } myCell)
            {
                UnityEngine.Debug.Log(" ".ThisManyTimes(4) + Owner.DebugName + " not in active zone.");
                return 0;
            }

            if (Occludes
                && !entityCell.HasLOSTo(myCell))
            {
                UnityEngine.Debug.Log(" ".ThisManyTimes(4) +
                    Owner.GetReferenceDisplayName(Stripped: true, Short: true) + 
                    " does not have LOS to " + 
                    Entity.GetReferenceDisplayName(Stripped: true, Short: true));
                return 0;
            }

            int distance = entityCell.CosmeticDistanceto(myCell.Location);
            ClampedRange score = Taper(distance);

            int roll = score.Roll();

            LastRoll = roll;
            LastEntityID = Entity.ID;

            UnityEngine.Debug.Log(" ".ThisManyTimes(4) +
                GetName() + "(" +
                nameof(roll) + ": " + roll + " | " +
                nameof(distance) + ": " + distance + " | " +
                nameof(Tapers) + ": " + Tapers + " | " +
                nameof(score) + ": " + score + ")");

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
            ClampedRange.WriteOptimized(Writer, BaseScore);
            Radius.WriteOptimized(Writer, BaseRadius);
            ClampedRange.WriteOptimized(Writer, Score);
            Radius.WriteOptimized(Writer, Radius);
        }
        public virtual void Read(SerializationReader Reader)
        {
            BaseScore = ClampedRange.ReadOptimizedClampedRange(Reader);
            BaseRadius = Radius.ReadOptimizedRadius(Reader);
            _Score = ClampedRange.ReadOptimizedClampedRange(Reader);
            _Radius = Radius.ReadOptimizedRadius(Reader);
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
