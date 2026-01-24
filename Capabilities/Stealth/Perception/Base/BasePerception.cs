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
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

using SerializeField = UnityEngine.SerializeField;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Perceptions
{
    [StealthSystemBaseClass]
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

        private IPurview _Purview;
        IPurview IPerception.Purview
        {
            get => _Purview ??= ((IPerception)this).GetPurview();
            set
            {
                _Purview = value;
                _Purview.SetParentPerception(this);
            }
        }

        #endregion
        #region Constructors

        public BasePerception()
        {
            _Name = null;
            _ShortName = null;

            Owner = null;

            _Level = 0;
            _Purview = null;
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
            this._Purview = Purview;
        }

        #endregion
        #region Serialization

        public abstract IPurview ReadPurview(SerializationReader Reader, IPerception ParentPerception);

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            Writer.WriteGameObject(Owner);
            Writer.WriteOptimized(Level);
            IPurview.WriteOptimized(Writer, _Purview);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            Owner = Reader.ReadGameObject();
            Level = Reader.ReadOptimizedInt32();
            _Purview = ReadPurview(Reader, this);
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

        public virtual void Remove()
        {
        }

        IPurview IPerception.GetPurview()
            => null;

        public virtual BasePerception DeepCopy(GameObject Parent)
        {
            BasePerception perception = Activator.CreateInstance(GetType()) as BasePerception;

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perception, fieldInfo.GetValue(this));

            perception.Owner = Parent;

            return perception;
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

        public virtual bool HandleEvent(BeforeAlertEvent E)
            => true;

        public virtual bool HandleEvent(AfterAlertEvent E)
            => true;

        #endregion

        public virtual string ToString(bool Short)
            => GetName(Short) + "[" + EffectiveLevel + ":@P:" + ((IPerception)this).Purview.EffectiveValue + "]";

        public override string ToString()
            => ToString(false);

        public virtual string GetName(bool Short = false)
            => GetType()?.ToStringWithGenerics(Short) ?? (Short ? "?" : "null?");

        public IDetection RaiseDetection(AlertContext AlertContext)
        {
            throw new NotImplementedException("Better implement this soon!");
        }

        public bool CheckInPurview(AlertContext Context)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.MiniDebugName() ?? "null"),
                    Debug.Arg(nameof(Context.Actor), Context?.Actor?.MiniDebugName() ?? "null"),
                });

            return Context?.AlertLocation is Cell alertLocation
                && _Purview is IPurview purview
                && purview.IsWithin(alertLocation);
        }

        public abstract bool CanPerceiveAlert(IAlert Alert);

        public virtual bool CanPerceive(AlertContext Context)
            => ((IPerception)this).CanPerceive(Context);

        public virtual bool TryPerceive(AlertContext Context)
        {
            if (Context == null)
                return false;

            if (!Validate())
                return false;

            if (!CanPerceiveAlert(Context.Alert))
                return false;

            return true;
        }

        public virtual void ClearCaches()
            => _Purview?.ClearCaches();

        public virtual bool Validate()
            => ((IPerception)this).Validate();

        public virtual int GetLevelAdjustment(int Level = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(Owner, this, Level);

        /*
        public bool RaiseAlert<TSense, TAlert>(AlertContext<TSense> Context, ISense<TSense> Sense, AwarenessLevel Level, bool? OverridesCombat = null)
            where TSense : ISense<TSense>, new()
            where TAlert : Detection<IPerception<TSense>, TSense>, new()
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

            if (Detection<IPerception<TSense>, TSense>.NewFromContext(Context, Sense, Level, OverridesCombat) is Detection<IPerception<TSense>, TSense> alert
                && BeforeAlertEvent.CheckHider(Context.Hider, ref alert)
                && BeforeAlertEvent.CheckPerceiver(Context.Perceiver, ref alert))
            {
                Context.Perceiver?.Brain.PushGoal(alert);
                return true;
            }
            return false;
        }
        */

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
