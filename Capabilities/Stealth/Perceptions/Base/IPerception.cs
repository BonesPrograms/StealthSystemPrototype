using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using System.Reflection;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class IPerception
        : IComponent<GameObject>,
        IComparable<IPerception>,
        IWitnessEventHandler,
        IPerceptionEventHandler
    {
        #region Helpers

        public class EventBinder : IEventBinder
        {
            public static readonly EventBinder Instance = new();

            public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
            {
                Writer.WriteGameObject(((IPerception)Handler).Owner, Reference: true);
                Writer.WriteTokenized(Handler.GetType());
            }

            public override IEventHandler ReadBind(SerializationReader Reader, int ID)
            {
                GameObject owner = Reader.ReadGameObject();
                Type type = Reader.ReadTokenizedType();
                foreach (IPerception perception in owner?.GetPerceptions() ?? new())
                    if ((object)perception.GetType() == type)
                        return perception;

                return null;
            }
        }

        public class RatingComparer : IComparer<IPerception>
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

            public virtual int Compare(IPerception x, IPerception y)
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
        #region Instance Fields & Properties

        public sealed override IEventBinder Binder => EventBinder.Instance;

        private string _Name;
        public string Name => _Name ??= GetName();

        private string _ShortName;
        public string ShortName => _ShortName ??= GetName(true);

        public GameObject Owner;

        public PerceptionRack Rack => Owner?.GetPerceptions();

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

        [NonSerialized]
        protected List<Cell> _RadiusAreaCells;

        public List<Cell> RadiusAreaCells => _RadiusAreaCells ??= GetRadiusAreaCells();

        #endregion
        #region Constructors

        public IPerception()
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

            _RadiusAreaCells = null;
        }
        public IPerception(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public IPerception(
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
        public IPerception(
            GameObject Owner,
            PerceptionSense Sense)
            : this(Owner, Sense, BASE_DIE_ROLL, BASE_RADIUS)
        {
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            ClampedDieRoll.WriteOptimized(Writer, BaseDieRoll);
            Radius.WriteOptimized(Writer, BaseRadius);
            ClampedDieRoll.WriteOptimized(Writer, DieRoll);
            Radius.WriteOptimized(Writer, Radius);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            BaseDieRoll = ClampedDieRoll.ReadOptimizedClampedRange(Reader);
            BaseRadius = Radius.ReadOptimizedRadius(Reader);
            _DieRoll = ClampedDieRoll.ReadOptimizedClampedRange(Reader);
            _Radius = Radius.ReadOptimizedRadius(Reader);
        }

        public virtual void FinalizeRead(SerializationReader Reader)
        {
        }

        #endregion

        public override GameObject GetComponentBasis()
            => Owner;

        #region Base Methods

        public virtual void Initialize()
        {
        }

        public virtual void Attach()
        {
        }

        public virtual void AddedAfterCreation()
        {
        }

        public virtual void Remove()
        {
        }

        public virtual IPerception DeepCopy(GameObject Parent)
        {
            IPerception perception = (IPerception)Activator.CreateInstance(GetType());

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perception, fieldInfo.GetValue(this));

            perception.Owner = Parent;

            return perception;
        }

        public virtual bool Validate()
        {
            if (Owner == null)
                return false;

            return true;
        }

        public virtual int GetBonusBaseDieRoll()
            => 0;

        public virtual int GetBonusBaseRadius()
            => 0;

        public virtual int GetBonusDieRoll()
            => 0;

        public virtual int GetBonusRadius()
            => 0;

        #endregion
        #region Virtual Event Registration

        public virtual void ApplyRegistrar(GameObject Object, bool Active = false)
        {
            if (Active)
            {
                RegisterActive(Object, EventRegistrar.Get(Object, this));
                return;
            }
            Register(Object, EventRegistrar.Get(Object, this));
            /* This is base game code which should be investigated and possibly emulated 
            if (ComponentReflection.EffectEvents.TryGetValue(GetType(), out var value))
            {
                Object.CurrentCell?.FlushRenderCache();
                for (int i = 0; i < value.Length; i++)
                    Object.RegisterEvent(this, value[i]);
            }
            */
            if (WantEvent(EndTurnEvent.ID, EndTurnEvent.CascadeLevel))
                Object.RegisterEvent(this, EndTurnEvent.ID);
        }

        public virtual void ApplyUnregistrar(GameObject Object, bool Active = false)
        {
            EventUnregistrar registrar = EventUnregistrar.Get(Object, this);
            if (!Active)
            {
                Register(Object, registrar);
                /* This is base game code which should be investigated and possibly emulated 
                if (ComponentReflection.PartEvents.TryGetValue(GetType(), out var value))
                    for (int i = 0; i < value.Length; i++)
                        Object.UnregisterEvent(this, value[i]);
                */
                if (WantEvent(EndTurnEvent.ID, EndTurnEvent.CascadeLevel))
                    Object.UnregisterEvent(this, EndTurnEvent.ID);
            }
            RegisterActive(Object, registrar);
        }

        /// <summary>Register to events from the <see cref="GameObject" /> while it is active in the action queue.</summary>
        /// <remarks>It is safer to register for external events here, since they're guaranteed to be cleaned up once the object goes out of scope.</remarks>
        /// <param name="Object">The current <see cref="GameObject" />.</param>
        /// <param name="Registrar">An <see cref="IEventRegistrar" /> with this <see cref="IPerception" /> and <see cref="GameObject" />  provisioned as defaults.</param>
        public virtual void RegisterActive(GameObject Object, IEventRegistrar Registrar)
        {
        }
        /// <summary>Register to events from the <see cref="GameObject" />.</summary>
        /// <param name="Object">The current <see cref="GameObject" />.</param>
        /// <param name="Registrar">An <see cref="IEventRegistrar" /> with this <see cref="IPerception" /> and <see cref="GameObject" /> provisioned as defaults.</param>
        public virtual void Register(GameObject Object, IEventRegistrar Registrar)
        {
        }

        #endregion
        #region Virtual HandleEvent

        public virtual bool HandleEvent(GetWitnessesEvent E)
            => true;

        // this currently unlikely to ever be called, based on how collection and dispatch happens,
        // but it is a goal to get it working.
        public virtual bool HandleEvent(GetPerceptionsEvent E)
            => true;

        public virtual bool HandleEvent(GetPerceptionDieRollEvent E)
            => true;

        public virtual bool HandleEvent(GetPerceptionRadiusEvent E)
            => true;

        #endregion

        protected ClampedDieRoll GetDieRoll<T>(T Perception = null)
            where T : IPerception
            => GetPerceptionDieRollEvent.GetFor(
                    Perceiver: Owner,
                    Perception: Perception ?? (T)this,
                    BaseDieRoll: BaseDieRoll.AdjustBy(GetBonusBaseDieRoll()))
                ?.AdjustBy(GetBonusDieRoll());

        protected Radius GetRadius<T>(T Perception = null)
            where T : IPerception
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
            return ShortName + "[" + BaseDieRoll + ":@R:" + BaseRadius + "]" + rollString;
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

        public virtual List<Cell> GetRadiusAreaCells()
            => Radius.IsArea()
                && Owner?.CurrentCell?.GetCellsInACosmeticCircle(Radius) is IEnumerable<Cell> cells
            ? Event.NewCellList(cells)
            : Event.NewCellList();

        public bool CheckInRadius(GameObject Entity, out int Distance, out FindPath PerceptionPath)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            PerceptionPath = null;
            Distance = default;

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Entity?.CurrentCell is not Cell { InActiveZone: true } entityCell)
            {
                Debug.CheckNah(nameof(Entity), "Not in active zone", Indent: indent[1]);
                return false;
            }

            if (Owner?.CurrentCell is not Cell { InActiveZone: true } myCell)
            {
                Debug.CheckNah(nameof(Owner), "Not in active zone", Indent: indent[1]);
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
                    Debug.CheckNah(
                        Message: Owner.MiniDebugName() +
                            " does not have LOS to " +
                            Entity.MiniDebugName(),
                        Indent: indent[1]);
            }
            if (Radius.IsArea())
            {
                if (RadiusAreaCells.Contains(entityCell))
                {
                    if (!Occludes
                        || entityCell.HasLOSTo(myCell))
                        any = true;
                    else
                        Debug.CheckNah(
                            Message: "Area around " +
                                Owner.MiniDebugName() +
                                " contains " +
                                Entity.MiniDebugName() +
                                ", but does not have LOS",
                            Indent: indent[1]);
                }
                else
                    Debug.CheckNah(
                        Message: "Area around " + 
                            Owner.MiniDebugName() +
                            " does not contain " +
                            Entity.MiniDebugName(),
                        Indent: indent[1]);
            }
            if (Radius.IsPathing())
            {
                PerceptionPath = new(myCell, entityCell);
                if (PerceptionPath.Steps is List<Cell> pathSteps)
                {
                    if (pathSteps.Count >= radiusValue)
                        any = true;
                    else
                        Debug.CheckNah(
                            Message: "Perception path from " +
                                Owner.MiniDebugName() + 
                                " is too long to reach " +
                                Entity.MiniDebugName() ,
                            Indent: indent[1]);
                }
            }
            Debug.YehNah(nameof(any), any, any, Indent: indent[1]);
            return any;
        }

        public virtual int Roll(GameObject Entity, bool UseLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

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

            roll = (int)Math.Floor(roll * Radius.GetDiffusion(distance));

            Debug.Log(nameof(roll), roll, Indent: indent[1]);
            Debug.Log(nameof(distance), distance, Indent: indent[1]);

            string diffussesString = Diffuses.ToString() + ", " + diffusion.WithDigits(3);
            string diffusionCountString = distance.Clamp(new(Radius.GetValue())) + "/" + (Radius.Diffusions()?.Count() ?? 0);
            Debug.Log(nameof(Diffuses), diffussesString + " (" + diffusionCountString + ")", Indent: indent[1]);
            Debug.Log(Radius.GetDiffusionDebug(Inline: false), Indent: indent[2]);

            Debug.Log(nameof(DieRoll), DieRoll, Indent: indent[1]);

            return roll;
        }
        public virtual int RollAdvantage(GameObject Entity, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            GetMinMax(out _, out int max, Roll(Entity, AgainstLastRoll), Roll(Entity, false));
            return max;
        }
        public virtual int RollDisadvantage(GameObject Entity, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            GetMinMax(out int min, out _, Roll(Entity, AgainstLastRoll), Roll(Entity, false));
            return min;
        }

        public static AwarenessLevel CalculateAwareness(int Roll)
            => (AwarenessLevel)((int)Math.Ceiling(((Roll + 1) / 20.0) - 1)).Clamp(0, 4);

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll, bool UseLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(UseLastRoll), UseLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(GetAwareness) + " requires a " + nameof(GameObject) + " to perceive.");

            if (UseLastRoll
                && LastRoll != null
                && LastEntityID == Entity.ID)
                Roll = LastRoll.Value;
            else
                Roll = this.Roll(Entity);

            AwarenessLevel awarenessLevel = CalculateAwareness(Roll);

            Debug.CheckYeh(awarenessLevel.ToStringWithNum(), Indent: indent[1]);

            return awarenessLevel;
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, bool UseLastRoll = false)
            => GetAwareness(Entity, out _);

        #region Event Handling

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == EnteredCellEvent.ID
            ;
        public override bool HandleEvent(EnteredCellEvent E)
        {
            _RadiusAreaCells = null;
            return base.HandleEvent(E);
        }

        #endregion
        #region Comparison

        public int CompareTo(IPerception other)
        {
            if (EitherNull(this, other, out int comparison))
                return comparison;

            int dieRollComp = (DieRoll.Average() - other.DieRoll.Average()) - (other.DieRoll.Breadth() - DieRoll.Breadth());
            if (dieRollComp != 0)
                return dieRollComp;

            return Radius.CompareTo(other.Radius);
        }

        #endregion
        #region Operator Overloads

        #region Comparison

        public static bool operator <(IPerception Op1, IPerception Op2)
            => Op1.CompareTo(Op2) < 0;

        public static bool operator >(IPerception Op1, IPerception Op2)
            => Op1.CompareTo(Op2) > 0;

        public static bool operator <=(IPerception Op1, IPerception Op2)
            => Op1.CompareTo(Op2) <= 0;

        public static bool operator >=(IPerception Op1, IPerception Op2)
            => Op1.CompareTo(Op2) >= 0;

        #endregion
        #endregion
    }
}
