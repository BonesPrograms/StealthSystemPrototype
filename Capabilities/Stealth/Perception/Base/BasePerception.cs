using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

using SerializeField = UnityEngine.SerializeField;
using XRL.Collections;

namespace StealthSystemPrototype.Perceptions
{
    [HasModSensitiveStaticCache]
    [StealthSystemBaseClass]
    [Serializable]
    public class BasePerception
        : IComponent<GameObject>
        , IPerception
        , IWitnessEventHandler
        , IPerceptionEventHandler
        , ISneakEventHandler
        , IDetectionEventHandler
    {
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Perceptions.BasePerception),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(ToString), false },
                });
        }
        #endregion
        #region Helpers

        public class EventBinder : IEventBinder
        {
            public static readonly EventBinder Instance = new();

            public override void WriteBind(SerializationWriter Writer, IEventHandler Handler, int ID)
            {
                Writer.WriteGameObject(((IPerception)Handler).GetOwner(), Reference: true);
                Writer.WriteTokenized(Handler.GetType());
            }

            protected IEventHandler GetHandler(GameObject Owner, Type Type)
                => (Owner?.GetPerceptions() ?? new())
                ?.FirstOrDefault(p => p.GetType() == Type);

            public override IEventHandler ReadBind(SerializationReader Reader, int ID)
                => Reader.ReadGameObject() is GameObject owner
                    && Reader.ReadTokenizedType() is Type type
                    && GetHandler(owner, type) is IEventHandler handler
                ? handler
                : null;
        }

        public class PerceptionComparer : IComparer<IPerception>
        {
            public enum ComparisonType
            {
                None,
                Level,
                EffectiveLevel,
                Purview,
            }
            protected ComparisonType Type;

            public PerceptionComparer()
            {
                Type = ComparisonType.None;
            }
            public PerceptionComparer(ComparisonType Type)
                : this()
            {
                this.Type = Type;
            }
            public virtual int Compare(IPerception x, IPerception y)
                => EitherNull(x, y, out int comparison)
                ? comparison
                : Type switch
                {
                    ComparisonType.Level => x.CompareLevelTo(y),
                    ComparisonType.EffectiveLevel => x.CompareEffectiveLevelTo(y),
                    ComparisonType.Purview => x.ComparePurviewTo(y),
                    ComparisonType.None or
                    _ => x.CompareTo(y),
                };
        }

        #endregion
        #region Const & Static Values

        [ModSensitiveStaticCache]
        public static Dictionary<Type, int[]> PerceptionEvents;

        [ModSensitiveCacheInit]
        public static void InitializeCache()
        {
            PerceptionEvents = new();

            Type[] renderMethodArgType = new Type[1] { typeof(RenderEvent) };
            Type[] turnTickArgTypes = new Type[2]
            {
                typeof(long),
                typeof(int)
            };

            List<Type> baseTypes = new()
            {
                typeof(IComponent<GameObject>),
            };

            if (ModManager.GetTypesWithAttribute(typeof(StealthSystemBaseClassAttribute)) is List<Type> stealthSystemBaseTypes
                && stealthSystemBaseTypes.Count > 0)
                baseTypes.AddRange(stealthSystemBaseTypes);

            IEnumerable<Type> perceptionTypes = ModManager.ActiveTypes
                .Where(t => typeof(IComponent<GameObject>).IsAssignableFrom(t))
                .Where(t => typeof(IPerception).IsAssignableFrom(t))
                .Where(t => !t.IsAbstract)
                ;

            Type[] baseTypesArray = baseTypes.ToArray();

            Rack<int> eventIDRack = new();
            foreach (Type perceptionType in perceptionTypes)
            {
                if (!perceptionType.GetMethod(nameof(Render), renderMethodArgType).DeclaringType.EqualsAny(baseTypesArray))
                    eventIDRack.Add(RenderEvent.ID);

                if (!perceptionType.GetMethod(nameof(OverlayRender), renderMethodArgType).DeclaringType.EqualsAny(baseTypesArray))
                    eventIDRack.Add(RenderEvent.OverlayID);

                if (!perceptionType.GetMethod(nameof(FinalRender), renderMethodArgType).DeclaringType.EqualsAny(baseTypesArray))
                    eventIDRack.Add(RenderEvent.FinalID);

                if (!perceptionType.GetMethod(nameof(TurnTick), turnTickArgTypes).DeclaringType.EqualsAny(baseTypesArray))
                    eventIDRack.Add(TurnTickID);

                if (eventIDRack.Count > 0)
                {
                    PerceptionEvents[perceptionType] = eventIDRack.ToArray();
                    eventIDRack.Clear();
                }
            }
        }

        #endregion
        #region Instance Fields & Properties

        public sealed override IEventBinder Binder => EventBinder.Instance;

        [SerializeField]
        private string _Name;
        public string Name => _Name ??= (GetType()?.ToStringWithGenerics() ?? "null?");

        [SerializeField]
        private string _ShortName;
        public string ShortName => _ShortName ??= (GetType()?.ToStringWithGenerics(true) ?? "?");

        [NonSerialized]
        protected GameObject _Owner;
        public virtual GameObject Owner
        {
            get => _Owner;
            set => _Owner = value;
        }

        public PerceptionRack Rack => Owner?.GetPerceptions();

        public int Level;

        [NonSerialized]
        protected int? _EffectiveLevel;
        public int EffectiveLevel
        {
            get
            {
                if (_EffectiveLevel == null
                    && !GettingLevelAdjustment)
                {
                    GettingLevelAdjustment.Toggle();

                    _EffectiveLevel = Level + GetLevelAdjustment(Level);

                    GettingLevelAdjustment.Toggle();
                }
                return _EffectiveLevel ?? Level;
            }
        }
        private bool GettingLevelAdjustment = false;

        private Type AlertType => null;

        [NonSerialized]
        protected BasePurview _Purview;
        public virtual BasePurview Purview
        {
            get => _Purview ??= new BasePurview();
            protected set => _Purview = value;
        }

        [NonSerialized]
        protected int _Cooldown;
        public virtual int Cooldown
        {
            get => _Cooldown.Clamp(0, MaxCooldown);
            set => _Cooldown = value.Clamp(0, MaxCooldown);
        }

        public virtual int MaxCooldown => 10;

        #endregion
        #region Constructors

        public BasePerception()
        {
            _Name = null;
            _ShortName = null;

            Owner = null;

            Level = 0;
            _EffectiveLevel = null;

            _Purview = null;

            Cooldown = 0;
        }
        public BasePerception(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public BasePerception(
            GameObject Owner,
            int Level)
            : this(Owner)
        {
            this.Level = Level;
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            Writer.WriteGameObject(Owner);
            Writer.WriteOptimized(Level);
            Writer.WriteComposite(Purview);
            Writer.WriteOptimized(Cooldown);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            Owner = Reader.ReadGameObject();
            Level = Reader.ReadOptimizedInt32();
            Purview = Reader.ReadComposite() as BasePurview;
            Cooldown = Reader.ReadOptimizedInt32();
        }

        public virtual void FinalizeRead(SerializationReader Reader)
        {
        }

        #endregion

        public override GameObject GetComponentBasis()
            => Owner;

        #region Base Methods

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack if indicated as initial.
        /// </summary>
        public virtual void Initialize()
        {
        }

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack.
        /// </summary>
        public virtual void Attach()
        {
        }

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack if indicated as not creation.
        /// </summary>
        public virtual void AddedAfterCreation()
        {
        }

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is removed from the rack.
        /// </summary>
        public virtual void Remove()
        {
        }

        /// <summary>
        /// Creates a deep copy of a <see cref="BasePerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="BasePerception"/> with values matching the original, and reassigned reference members.</returns>
        public virtual BasePerception DeepCopy(GameObject Owner)
        {
            BasePerception perception = Activator.CreateInstance(GetType()) as BasePerception;

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perception, fieldInfo.GetValue(this));

            perception.Owner = Owner;
            perception.Purview = null;
            perception.ClearCaches();

            return perception;
        }

        /// <summary>
        /// Creates a deep copy of an <see cref="IPerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="IPerception"/> with values matching the original, and reassigned reference members.</returns>
        IPerception IPerception.DeepCopy(GameObject Owner)
            => DeepCopy(Owner);

        #endregion

        public virtual string GetName(bool Short = false)
            => !Short
            ? Name
            : ShortName;

        public virtual GameObject GetOwner()
            => Owner;

        public virtual Type GetAlertType()
            => AlertType;

        public virtual int GetLevel()
            => Level;

        public virtual int GetLevelAdjustment(int Level = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(Owner, this, Level);

        public virtual int GetEffectiveLevel()
            => EffectiveLevel;

        public virtual BasePurview GetPurview()
            => Purview;

        IPurview IPerception.GetPurview()
            => GetPurview();

        public virtual int GetCooldown()
            => Cooldown;

        public virtual int GetMaxCoolDown()
            => MaxCooldown;

        public virtual string ToString(bool Short)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(Short), Short),
                });

            return (Short ? ShortName : Name) + 
                "(" + Level + "/" + EffectiveLevel + "):" +
                "@P:" + (_Purview?.ToString() ?? "NO_PURVIEW[-0]");
        }

        public override string ToString()
            => ToString(false);

        public virtual bool SameAs(IPerception Other)
            => SameAlertAs(Other);

        public virtual bool SameAlertAs(IPerception Other)
            => GetAlertType() == Other.GetAlertType();

        public virtual void ConfigurePurview(int Value, Dictionary<string, object> args = null)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(Value), Value),
                    Debug.Arg(nameof(args), args?.Count ?? 0),
                });

            args ??= new();
            args[nameof(Value)] = Value;

            if (GetPurview() is BasePurview purview)
            {
                Debug.CheckYeh(nameof(purview), Indent: indent[1]);
                purview.Configure(args);
            }
            else
            {
                Debug.CheckYeh(nameof(purview), Indent: indent[1]);
            }
        }

        public bool IsCompatibleWith(BasePurview Purview)
            => GetAlertType() == Purview.AlertType;

        bool IPerception.IsCompatibleWith(IPurview Purview)
            => Purview is BasePurview purview
            && IsCompatibleWith(purview);

        public virtual bool CheckInPurview(AlertContext Context)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.MiniDebugName() ?? "null"),
                    Debug.Arg(nameof(Context.Hider), Context?.Hider?.MiniDebugName() ?? "null"),
                });

            return Purview?.IsWithin(Context) ?? false;
        }

        public virtual bool IsOnCooldown()
            => Cooldown > 0;

        public virtual void TickCooldown()
            => (--Cooldown).Clamp(0, MaxCooldown);

        public virtual void GoOnCooldown(int Cooldown)
            => this.Cooldown = Cooldown.Clamp(0, MaxCooldown);

        public virtual void GoOnCooldown()
            => Cooldown = MaxCooldown;

        public virtual void GoOffCooldown()
            => Cooldown = 0;

        public virtual bool CanPerceiveAlert(BaseAlert Alert)
            => Alert?.IsType(GetAlertType()) ?? false;

        bool IPerception.CanPerceiveAlert(IAlert Alert)
            => Alert is BaseAlert alert
            && CanPerceiveAlert(alert);

        public virtual bool CanPerceive(AlertContext Context)
            => CanPerceiveAlert(Context?.ActionAlert);

        public virtual bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin)
        {
            SuccessMargin = 0;
            FailureMargin = 0;

            if (Context == null)
                return false;

            if (!Validate())
                return false;

            if (!CanPerceiveAlert(Context.ActionAlert))
                return false;

            if (IsOnCooldown())
                return false;

            bool madeSave = IPerception.MakeSave(
                SuccessMargin: out SuccessMargin,
                FailureMargin: out FailureMargin,
                Perception: this,
                Context: Context);

            if (!madeSave)
            {
                GoOnCooldown(FailureMargin);
                return false;
            }
            Context.SetPerception(this);
            RaiseDetection(Context, SuccessMargin);
            return true;
        }

        public virtual IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ToString()),
                    Debug.Arg(nameof(Context.Perceiver), Context?.Perceiver.MiniDebugName()),
                    Debug.Arg(nameof(Context.Hider), Context?.Hider.MiniDebugName()),
                });

            AwarenessLevel level = AwarenessLevel.Aware;

            if (SuccessMargin >= 10)
                level++;
            if (SuccessMargin >= 20)
                level = AwarenessLevel.Alert;

            return Owner.Brain.AddOpinionDetection(
                Detection: GetDetectionOpinionEvent.GetFor(
                    Perceiver: Owner,
                    Hider: Context.Hider,
                    Detection: new Curious(),
                    Level: ref level),
                Context: Context,
                Level: level);
        }

        public virtual void ClearCaches()
        {
            _EffectiveLevel = null;
            _Purview?.ClearCaches();
        }

        public virtual bool Validate()
            => Owner != null;

        #region Event Handling

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == EnteredCellEvent.ID
            ;
        public override bool HandleEvent(EnteredCellEvent E)
        {
            ClearCaches();
            return base.HandleEvent(E);
        }

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

            if (PerceptionEvents.TryGetValue(GetType(), out int[] cachedEventIDs))
            {
                Object.CurrentCell?.FlushRenderCache();
                for (int i = 0; i < cachedEventIDs.Length; i++)
                    Object.RegisterEvent(this, cachedEventIDs[i]);
            }

            if (WantEvent(EndTurnEvent.ID, EndTurnEvent.CascadeLevel))
                Object.RegisterEvent(this, EndTurnEvent.ID);
        }

        public virtual void ApplyUnregistrar(GameObject Object, bool Active = false)
        {
            EventUnregistrar registrar = EventUnregistrar.Get(Object, this);
            if (!Active)
            {
                Register(Object, registrar);

                if (PerceptionEvents.TryGetValue(GetType(), out int[] cachedEventIDs))
                    for (int i = 0; i < cachedEventIDs.Length; i++)
                        Object.UnregisterEvent(this, cachedEventIDs[i]);

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

        public virtual bool HandleEvent(GetPerceptionsEvent E)
            => true;

        public virtual bool HandleEvent(AdjustTotalPerceptionLevelEvent E)
            => true;

        public virtual bool HandleEvent(AdjustTotalPurviewEvent E)
            => true;

        public virtual bool HandleEvent(BeforeSneakEvent E)
            => true;

        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
            => true;

        public virtual bool HandleEvent(GetSneakDetailsEvent E)
            => true;

        public virtual bool HandleEvent(GetActionAlertsEvent E)
            => true;

        public virtual bool HandleEvent(TryConcealActionEvent E)
            => true;

        public virtual bool HandleEvent(GetDetectionOpinionEvent E)
            => true;

        public virtual bool HandleEvent(BeforeDetectedEvent E)
            => true;

        public virtual bool HandleEvent(AfterDetectedEvent E)
            => true;

        #endregion
        #region Comparison

        public virtual int CompareLevelTo(IPerception Other)
            => ((IPerception)this).CompareLevelTo(Other);

        public virtual int CompareEffectiveLevelTo(IPerception Other)
            => ((IPerception)this).CompareEffectiveLevelTo(Other);

        public virtual int ComparePurviewTo(IPerception Other)
            => ((IPerception)this).ComparePurviewTo(Other);

        public virtual int CompareTo(IPerception Other)
            => ((IPerception)this).CompareLevelTo(Other);

        #endregion
    }
}
