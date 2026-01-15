using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;

using StealthSystemPrototype.Events;
using static StealthSystemPrototype.Utils;
using XRL.World.AI.Pathfinding;

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

        public const int MIN_DIE_ROLL = 0;
        public const int MAX_DIE_ROLL = 100;
        public static ClampedDieRoll BASE_DIE_ROLL => new(30..50, DIE_ROLL_CLAMP); // ~10 either side of AwarenessLevel.Suspect

        public const int MIN_RADIUS = 0;
        public const int MAX_RADIUS = 84; // corner to corner of a single zone.
        public static Radius BASE_RADIUS => new(10, RADIUS_CLAMP, Radius.DefaultDiffuser);

        public static InclusiveRange DIE_ROLL_CLAMP => new(MIN_DIE_ROLL, MAX_DIE_ROLL);
        public static InclusiveRange RADIUS_CLAMP => new(MIN_RADIUS, MAX_RADIUS);

        public static Radius.RadiusFlags VisualFlag => Radius.RadiusFlags.Line | Radius.RadiusFlags.Occludes | Radius.RadiusFlags.Diffuses;
        public static Radius.RadiusFlags AuditoryFlag => Radius.RadiusFlags.Area | Radius.RadiusFlags.Pathing | Radius.RadiusFlags.Diffuses;
        public static Radius.RadiusFlags OlfactoryFlag => Radius.RadiusFlags.Area | Radius.RadiusFlags.Pathing | Radius.RadiusFlags.Diffuses;
        public static Radius.RadiusFlags PsionicFlag => Radius.RadiusFlags.Line | Radius.RadiusFlags.Area | Radius.RadiusFlags.Diffuses;

        #endregion

        #region Instance PropFields

        public string Name => GetName();
        public string ShortName => GetName(true);

        public GameObject Owner;

        public PerceptionSense Sense;

        [NonSerialized]
        public ClampedDieRoll BaseDieRoll;

        [NonSerialized]
        public Radius BaseRadius;

        protected ClampedDieRoll _DieRoll;
        public ClampedDieRoll DieRoll => _DieRoll ??= GetDieRoll(this);

        protected Radius _Radius;
        public Radius Radius => _Radius ??= GetRadius(this);

        public Radius.RadiusFlags RadiusFlags => Radius.Flags;
        public bool Occludes => Radius.Occludes();
        public bool Diffuses => Radius.Diffuses();

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

            BaseDieRoll = BASE_DIE_ROLL;
            BaseRadius = BASE_RADIUS;

            _DieRoll = null;
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
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : this(Owner)
        {
            this.Sense = Sense;
            this.BaseDieRoll = BaseDieRoll;
            this.BaseRadius = BaseRadius;
        }
        public BasePerception(
            GameObject Owner,
            PerceptionSense Sense)
            : this(Owner, Sense, BASE_DIE_ROLL, BASE_RADIUS)
        {
        }

        #endregion

        #region Abstract Methods

        public abstract bool Validate(GameObject Owner = null);

        public virtual int GetBonusBaseDieRoll() => 0;
        public virtual int GetBonusBaseRadius() => 0;

        public virtual int GetBonusDieRoll() => 0;
        public virtual int GetBonusRadius() => 0;

        #endregion

        protected ClampedDieRoll GetDieRoll<T>(T Perception = null)
            where T : BasePerception
            => GetPerceptionDieRollEvent.GetFor(
                    Perceiver: Owner,
                    Perception: Perception ?? (T)this,
                    BaseDieRoll: BaseDieRoll.AdjustBy(GetBonusBaseDieRoll()))
                ?.AdjustBy(GetBonusDieRoll());

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
            return GetName(Short) + "[" + BaseDieRoll + ":@R:" + BaseRadius + "]" + rollString;
        }

        public override string ToString()
            => ToString(false);

        public void ClearDieRoll()
            => _DieRoll = null;

        public void ClearRadius()
            => _Radius = null;

        public void ClearRating()
        {
            ClearDieRoll();
            ClearRadius();
        }

        public int CompareTo(BasePerception other)
        {
            if (EitherNull(this, other, out int comparison))
                return comparison;

            int dieRollComp = (DieRoll.Average() - other.DieRoll.Average()) - (other.DieRoll.Breadth() - DieRoll.Breadth());
            if (dieRollComp != 0)
                return dieRollComp;

            return Radius.CompareTo(other.Radius);
        }

        public bool CheckInRadius(GameObject Entity, out int Distance, out FindPath PerceptionPath)
        {
            PerceptionPath = null;
            Distance = default;

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Entity?.CurrentCell is not Cell { InActiveZone: true } entityCell)
            {
                UnityEngine.Debug.Log(" ".ThisManyTimes(4) + Entity.DebugName + " not in active zone.");
                return false;
            }

            if (Owner?.CurrentCell is not Cell { InActiveZone: true } myCell)
            {
                UnityEngine.Debug.Log(" ".ThisManyTimes(4) + Owner.DebugName + " not in active zone.");
                return false;
            }

            bool any = false;
            Distance = entityCell.CosmeticDistanceto(myCell.Location);
            int radiusValue = Radius.EffectiveValue;

            if (Radius.IsLine())
            {
                if (!Occludes
                    || entityCell.HasLOSTo(myCell))
                    any = radiusValue >= Distance || any;
                else
                    UnityEngine.Debug.Log(" ".ThisManyTimes(4) +
                        Owner.MiniDebugName() +
                        " does not have LOS to " +
                        Entity.MiniDebugName());
            }
            if (Radius.IsArea())
            {
                if (myCell.GetCellsInACosmeticCircle(Radius).Contains(entityCell))
                    any = (!Occludes || entityCell.HasLOSTo(myCell)) || any;
            }
            if (Radius.IsPathing())
            {
                PerceptionPath = new(myCell, entityCell);
                if (PerceptionPath.Steps is List<Cell> pathSteps)
                    any = pathSteps.Count >= radiusValue || any;
            }
            return any;
        }

        public virtual int Roll(GameObject Entity, bool UseLastRoll = false)
        {
            if (!CheckInRadius(Entity, out int distance, out FindPath perceptionPath))
                return 0;

            if (UseLastRoll
                && Entity.ID == LastEntityID
                && LastRoll is int lastRoll)
                return lastRoll;

            int roll = DieRoll.Roll();

            LastRoll = roll;
            LastEntityID = Entity.ID;

            double diffusion = Radius.GetDiffusion(distance);

            roll = (int)(roll * Radius.GetDiffusion(distance));

            UnityEngine.Debug.Log(" ".ThisManyTimes(4) +
                GetName() + "(" +
                nameof(roll) + ": " + roll + " | " +
                nameof(distance) + ": " + distance + " | " +
                nameof(Diffuses) + ": " + Diffuses.ToString() + ", " + String.Format("{0:0.000}", diffusion) + " (" + distance.Clamp(new(Radius.GetValue())) + "/" + (Radius.Diffusions()?.Count() ?? 0) + ") | " +
                nameof(DieRoll) + ": " + DieRoll + ")");

            UnityEngine.Debug.Log(" ".ThisManyTimes(8) +
                Radius.GetDiffusionDebug());

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
            ClampedDieRoll.WriteOptimized(Writer, BaseDieRoll);
            Radius.WriteOptimized(Writer, BaseRadius);
            ClampedDieRoll.WriteOptimized(Writer, DieRoll);
            Radius.WriteOptimized(Writer, Radius);
        }
        public virtual void Read(SerializationReader Reader)
        {
            BaseDieRoll = ClampedDieRoll.ReadOptimizedClampedRange(Reader);
            BaseRadius = Radius.ReadOptimizedRadius(Reader);
            _DieRoll = ClampedDieRoll.ReadOptimizedClampedRange(Reader);
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
