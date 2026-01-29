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
    public abstract class BasePerception
        : IComponent<GameObject>
        , IAlertTypedPerception
        , IWitnessEventHandler
        , IPerceptionEventHandler
        , ISneakEventHandler
        , IDetectionEventHandler
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
        public string Name => _Name ??= GetName();

        [SerializeField]
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

        public int EffectiveLevel => Level + GetLevelAdjustment(Level);

        [NonSerialized]
        protected IPurview _Purview;
        IPurview IPerception.Purview
        {
            get => _Purview;
            set => _Purview = value;
        }

        protected int _Cooldown;
        public virtual int Cooldown
        {
            get => _Cooldown;
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

            _Level = 0;
            _Purview = null;

            _Cooldown = 0;
        }
        public BasePerception(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public BasePerception(
            GameObject Owner,
            int Level,
            IPurview Purview)
            : this(Owner)
        {
            this.Level = Level;
            _Purview = Purview;
        }

        #endregion
        #region Serialization

        public abstract void WritePurview(SerializationWriter Writer, IPurview Purview);

        public abstract void ReadPurview(SerializationReader Reader, ref IPurview Purview, IPerception ParentPerception = null);

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            Writer.WriteGameObject(Owner);
            Writer.WriteOptimized(Level);
            WritePurview(Writer, _Purview);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            Owner = Reader.ReadGameObject();
            Level = Reader.ReadOptimizedInt32();
            ReadPurview(Reader, ref _Purview, this);
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
        /// Creates a deep copy of an <see cref="IPerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="IPerception"/> with values matching the original, and reassigned reference members.</returns>
        public virtual IPerception DeepCopy(GameObject Owner)
        {
            BasePerception perception = Activator.CreateInstance(GetType()) as BasePerception;

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perception, fieldInfo.GetValue(this));

            perception.Owner = Owner;

            return perception;
        }

        public virtual bool SameAs(IPerception Other)
            => Other is IAlertTypedPerception typedOther
            && SameAlertAs(typedOther);

        public virtual bool SameAlertAs(IAlertTypedPerception Other)
            => ((IAlertTypedPerception)this).SameAlertAs(Other);

        public abstract Type GetAlertType();

        public virtual void AssignDefaultPurview(int Value)
            => ((IPerception)this).Purview = GetDefaultPurview(Value);

        public abstract IPurview GetDefaultPurview(int Value);

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

        public virtual bool HandleEvent(TryConcealActionEvent E)
            => true;

        public virtual bool HandleEvent(BeforeDetectedEvent E)
            => true;

        public virtual bool HandleEvent(AfterDetectedEvent E)
            => true;

        #endregion

        public virtual string ToString(bool Short)
            => GetName(Short) + "[" + EffectiveLevel + ":@P:" + ((IPerception)this).Purview.EffectiveValue + "]";

        public override string ToString()
            => ToString(false);

        public virtual string GetName(bool Short = false)
            => GetType()?.ToStringWithGenerics(Short) ?? (Short ? "?" : "null?");

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

        public virtual bool CheckInPurview(AlertContext Context)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.MiniDebugName() ?? "null"),
                    Debug.Arg(nameof(Context.Hider), Context?.Hider?.MiniDebugName() ?? "null"),
                });

            return _Purview is IPurview purview
                && purview.IsWithin(Context);
        }

        public abstract bool CanPerceiveAlert(IAlert Alert);

        public virtual bool CanPerceive(AlertContext Context)
            => ((IPerception)this).CanPerceive(Context);

        public virtual bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin)
        {
            SuccessMargin = 0;
            FailureMargin = 0;

            if (Context == null)
                return false;

            if (!Validate())
                return false;

            if (!CanPerceiveAlert(Context.Alert))
                return false;

            if (IsOnCooldown())
                return false;

            bool madeSave = IPerception.MakeSave(
                SuccessMargin: out SuccessMargin,
                FailureMargin: out FailureMargin,
                Context: Context,
                BaseDifficulty: 10);

            if (!madeSave)
            {
                GoOnCooldown(SuccessMargin);
                return false;
            }
            RaiseDetection(Context, FailureMargin);
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
            => _Purview?.ClearCaches();

        public virtual bool Validate()
            => ((IPerception)this).Validate();

        public virtual int GetLevelAdjustment(int Level = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(Owner, this, Level);

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
    }
}
