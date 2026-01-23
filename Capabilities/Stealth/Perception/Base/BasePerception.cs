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
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public abstract class BasePerception
        : IComponent<GameObject>
        , IPerception
        , IWitnessEventHandler
        , IPerceptionEventHandler
        , ISneakEventHandler
        , IAlertEventHandler
    {
        #region Helpers

        public class EventBinder : IEventBinder
        {
            public static readonly EventBinder Instance = new();

            public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
            {
                Writer.WriteGameObject(((BasePerception)Handler).Owner, Reference: true);
                Writer.WriteTokenized(Handler.GetType());
            }

            public override IEventHandler ReadBind(SerializationReader Reader, int ID)
            {
                GameObject owner = Reader.ReadGameObject();
                Type type = Reader.ReadTokenizedType();
                foreach (BasePerception perception in owner?.GetPerceptions() ?? new())
                    if ((object)perception.GetType() == type)
                        return perception;

                return null;
            }
        }

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
                if (EitherNull(x, y, out int comparison))
                    return comparison;

                if (Entity != null)
                {
                    int rollComp = x.Roll(Entity).CompareTo(y.Roll(Entity));
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
        public static Purview BASE_RADIUS => new(10, RADIUS_CLAMP, Purview.DefaultDiffuser);

        public static InclusiveRange DIE_ROLL_CLAMP => new(MIN_DIE_ROLL, MAX_DIE_ROLL);
        public static InclusiveRange RADIUS_CLAMP => new(MIN_RADIUS, MAX_RADIUS);

        public static Purview.RadiusFlags VisualFlag => Purview.RadiusFlags.Line | Purview.RadiusFlags.Occludes | Purview.RadiusFlags.Diffuses;
        public static Purview.RadiusFlags AuditoryFlag => Purview.RadiusFlags.Area | Purview.RadiusFlags.Pathing | Purview.RadiusFlags.Diffuses;
        public static Purview.RadiusFlags OlfactoryFlag => Purview.RadiusFlags.Area | Purview.RadiusFlags.Pathing | Purview.RadiusFlags.Diffuses;
        public static Purview.RadiusFlags PsionicFlag => Purview.RadiusFlags.Line | Purview.RadiusFlags.Area | Purview.RadiusFlags.Diffuses;

        #endregion
        #region Instance Fields & Properties

        public sealed override IEventBinder Binder => EventBinder.Instance;

        private string _Name;
        public string Name => _Name ??= GetName();

        private string _ShortName;
        public string ShortName => _ShortName ??= GetName(true);

        private GameObject _Owner;
        public GameObject Owner
        {
            get => _Owner;
            set => _Owner = value;
        }

        public PerceptionRack Rack => Owner?.GetPerceptions();

        [NonSerialized]
        protected int _Level;
        public int Level
        {
            get => _Level;
            set => _Level = value;
        }

        protected Purview _Radius;
        public Purview Radius
        {
            get => _Radius;
            set => _Radius = value;
        }

        public Purview.RadiusFlags RadiusFlags => Radius.Flags;
        public bool Occludes => Radius.Occludes();
        public bool Diffuses => Radius.Diffuses();

        protected int? LastRoll;
        protected string LastEntityID;

        [NonSerialized]
        protected bool WantsToClearRating;

        [NonSerialized]
        protected IEnumerable<Cell> _RadiusAreaCells;

        public IEnumerable<Cell> RadiusAreaCells => _RadiusAreaCells ??= GetRadiusAreaCells();

        #endregion
        #region Constructors

        public BasePerception()
        {
            Owner = null;

            _Sense = null;

            BaseDieRoll = BASE_DIE_ROLL;
            BaseRadius = BASE_RADIUS;

            _DieRoll = null;
            _Radius = null;

            LastRoll = null;
            LastEntityID = null;

            WantsToClearRating = false;

            _RadiusAreaCells = null;
        }
        public BasePerception(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public BasePerception(
            GameObject Owner,
            ClampedDieRoll BaseDieRoll,
            Purview BaseRadius)
            : this(Owner)
        {
            this.BaseDieRoll = BaseDieRoll;
            this.BaseRadius = BaseRadius;
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            Writer.WriteGameObject(Owner);
            ClampedDieRoll.WriteOptimized(Writer, BaseDieRoll);
            Radius.WriteOptimized(Writer, BaseRadius);
            ClampedDieRoll.WriteOptimized(Writer, DieRoll);
            Purview.WriteOptimized(Writer, Radius);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            Owner = Reader.ReadGameObject();
            BaseDieRoll = ClampedDieRoll.ReadOptimizedClampedRange(Reader);
            BaseRadius = Purview.ReadOptimizedRadius(Reader);
            _DieRoll = ClampedDieRoll.ReadOptimizedClampedRange(Reader);
            _Radius = Purview.ReadOptimizedRadius(Reader);
        }

        public virtual void FinalizeRead(SerializationReader Reader)
        {
        }

        #endregion

        public override GameObject GetComponentBasis()
            => Owner;

        #region Base Methods

        public abstract void Initialize();

        public abstract void Attach();

        public abstract void AddedAfterCreation();

        public abstract void Remove();

        public virtual BasePerception DeepCopy(GameObject Parent)
        {
            BasePerception perception = (BasePerception)Activator.CreateInstance(GetType());

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

        public abstract int GetBonusBaseDieRoll();

        public abstract int GetBonusBaseRadius();

        public abstract int GetBonusDieRoll();

        public abstract int GetBonusRadius();

        protected abstract Type GetSenseType();

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
        /// <param name="Registrar">An <see cref="IEventRegistrar" /> with this <see cref="BasePerception" /> and <see cref="GameObject" />  provisioned as defaults.</param>
        public virtual void RegisterActive(GameObject Object, IEventRegistrar Registrar)
        {
        }
        /// <summary>Register to events from the <see cref="GameObject" />.</summary>
        /// <param name="Object">The current <see cref="GameObject" />.</param>
        /// <param name="Registrar">An <see cref="IEventRegistrar" /> with this <see cref="BasePerception" /> and <see cref="GameObject" /> provisioned as defaults.</param>
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

        public virtual bool HandleEvent(BeforeSneakEvent E)
            => true;

        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
            => true;

        public virtual bool HandleEvent(GetSneakDetailsEvent E)
            => true;

        public virtual bool HandleEvent(TryConcealActionEvent E)
            => true;

        public virtual bool HandleEvent(BeforeAlertEvent E)
            => true;

        public virtual bool HandleEvent(AfterAlertEvent E)
            => true;

        #endregion

        public static bool IsForSense(BasePerception Perception, Type SenseType, bool IncludeDerived = false)
            => EitherNull(Perception?.Sense, SenseType?.GetType(), out bool areEqual)
            ? areEqual
            : SenseType.InheritsFrom<ISense>()
                && (Perception.Sense == SenseType
                    || (IncludeDerived
                       && Perception.Sense.InheritsFrom(SenseType)));

        public static bool IsForSense(BasePerception Perception, ISense Sense, bool IncludeDerived = false)
            => IsForSense(Perception, Sense.GetType(), IncludeDerived);

        public bool IsForSense(Type SenseType, bool IncludeDerived = false)
            => IsForSense(this, SenseType, IncludeDerived);

        public bool IsForSense(ISense Sense, bool IncludeDerived = false)
            => IsForSense(this, Sense, IncludeDerived);

        protected ClampedDieRoll GetDieRoll<TSense>(IPerception<TSense> Perception = null)
            where TSense : ISense<TSense>, new()
            => GetPerceptionDieRollEvent.GetFor(
                    Perceiver: Owner,
                    Perception: Perception ?? (IPerception<TSense>)this,
                    BaseDieRoll: BaseDieRoll.AdjustBy(GetBonusBaseDieRoll()))
                ?.AdjustBy(GetBonusDieRoll());

        protected Purview GetRadius<TSense>(IPerception<TSense> Perception = null)
            where TSense : ISense<TSense>, new()
            => GetPerceptionRadiusEvent.GetFor(
                    Perceiver: Owner,
                    Perception: Perception ?? (IPerception<TSense>)this,
                    BaseRadius: BaseRadius.AdjustBy(GetBonusBaseRadius()))
                ?.AdjustBy(GetBonusRadius());

        public virtual string GetName(bool Short = false)
            => GetType()?.ToStringWithGenerics(Short) ?? (Short ? "?" : "null?");

        public virtual string ToString(bool Short, GameObject Entity = null, bool UseLastRoll = false)
        {
            string rollString = null;
            if (Entity != null)
                rollString = "(" + Roll(Entity) + ")";

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

        public bool CheckInRadius(GameObject Entity, out int Distance, out FindPath PerceptionPath, int Intensity = 0)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                    Debug.Arg(nameof(Intensity), Intensity),
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
            Distance = Math.Max(0, entityCell.CosmeticDistanceto(myCell.Location) - Intensity);
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

        public virtual int Roll(GameObject Entity)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                return 0;

            return DieRoll.Roll();;
        }

        public int[] Rolls(GameObject Entity, int Rolls = 1)
        {
            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(this.Rolls) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Rolls < 1)
                throw new ArgumentOutOfRangeException(nameof(Rolls), "Must be greater than or equal to 1.");

            int[] rolls = new int[Rolls];
            for (int i = 0; i < Rolls; i++)
                rolls[i] = Roll(Entity);

            return rolls;
        }

        public virtual int RollAdvantage(GameObject Entity, int Rolls = 2)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Rolls), Rolls),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(RollAdvantage) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Rolls < 1)
                throw new ArgumentOutOfRangeException(nameof(Rolls), "Must be greater than or equal to 1.");

            GetMinMax(out int _, out int max, this.Rolls(Entity, Rolls));
            return max;
        }
        public virtual int RollDisadvantage(GameObject Entity, int Rolls = 2)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Rolls), Rolls),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(RollDisadvantage) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Rolls < 1)
                throw new ArgumentOutOfRangeException(nameof(Rolls), "Must be greater than or equal to 1.");

            GetMinMax(out int min, out int _, this.Rolls(Entity, Rolls));
            return min;
        }

        public bool RaiseAlert<TSense, TAlert>(SenseContext<TSense> Context, ISense<TSense> Sense, AwarenessLevel Level, bool? OverridesCombat = null)
            where TSense : ISense<TSense>, new()
            where TAlert : IAlert<IPerception<TSense>, TSense>, new()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner.MiniDebugName()),
                    Debug.Arg(nameof(Sense), Sense?.Name ?? "NO_SENSE"),
                    Debug.Arg(nameof(Level), Level.ToStringWithNum()),
                    Debug.Arg(YehNah(OverridesCombat), nameof(OverridesCombat)),
                });

            if ((Context?.Perceiver?.Brain?.FindAlert(Context?.TypedPerception)?.Level ?? AwarenessLevel.None) > Level)
                return false;

            if (IAlert<IPerception<TSense>, TSense>.NewFromContext(Context, Sense, Level, OverridesCombat) is IAlert<IPerception<TSense>, TSense> alert
                && BeforeAlertEvent.CheckHider(Context.Hider, ref alert)
                && BeforeAlertEvent.CheckPerceiver(Context.Perceiver, ref alert))
            {
                Context.Perceiver?.Brain.PushGoal(alert);
                return true;
            }
            return false;
        }

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

        public int CompareTo(BasePerception Other)
        {
            if (EitherNull(this, Other, out int comparison))
                return comparison;

            int dieRollComp = (DieRoll.Average() - Other.DieRoll.Average()) - (Other.DieRoll.Breadth() - DieRoll.Breadth());
            if (dieRollComp != 0)
                return dieRollComp;

            return Radius.CompareTo(Other.Radius);
        }

        #endregion
        #region Operator Overloads

        #region Comparison

        public static bool operator <(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) < 0;

        public static bool operator >(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) > 0;

        public static bool operator <=(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) <= 0;

        public static bool operator >=(BasePerception Op1, BasePerception Op2)
            => Op1.CompareTo(Op2) >= 0;

        #endregion
        #endregion
    }
}
