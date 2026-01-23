using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Genkit;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;

using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public abstract class IAlert : GoalHandler, IComposite//, IComparable<BaseAlert>
    {
        [Serializable]
        protected class AlertSource : IComposite
        {
            public IAlert ParentAlert;

            private GameObject _Object;
            public GameObject Object => _Object;

            private Cell _Cell;
            public Cell Cell
            {
                get => _Cell == null
                        && !ZoneID.IsNullOrEmpty()
                        && The.ZoneManager is ZoneManager zoneManager
                        && zoneManager.IsZoneLive(ZoneID)
                    ? _Cell = zoneManager.GetZone(ZoneID).GetCell(Location)
                    : _Cell;
                set
                {
                    _Cell = value;
                    ZoneID = _Cell?.ParentZone?.ZoneID;
                    Location = _Cell?.Location ?? default;
                }
            }

            private string ZoneID;
            private Location2D Location;

            private Brain ParentBrain => ParentAlert?.ParentBrain;
            private GameObject ParentObject => ParentBrain?.ParentObject;

            #region Constructors

            public AlertSource()
            {
                ParentAlert = null;
                _Object = null;
                ZoneID = null;
                Location = default;
            }
            public AlertSource(IAlert ParentAlert, GameObject Object, Cell Cell)
                : this()
            {
                this.ParentAlert = ParentAlert;
                _Object = Object;
                this.Cell = Cell;
            }
            public AlertSource(IAlert ParentAlert, GameObject Object)
                : this(ParentAlert, Object, Object?.CurrentCell)
            {
            }
            public AlertSource(IAlert ParentAlert, Cell Cell)
                : this(ParentAlert, null, Cell)
            {
            }
            public AlertSource(IAlert ParentAlert, SenseContext Context)
                : this(ParentAlert, Context.Perceiver, Context.Perceiver?.CurrentCell)
            {
            }
            public AlertSource(AlertSource Source)
                : this(Source.ParentAlert, Source.Object, Source.Cell)
            {
            }
            public AlertSource(IAlert ParentAlert)
                : this(ParentAlert.Source)
            {
                SetParentAlert(ParentAlert);
            }

            public AlertSource SetParentAlert(IAlert Alert)
            {
                ParentAlert = Alert;
                return this;
            }

            #endregion
            #region Serialization

            public void Write(SerializationWriter Writer)
            {
                Writer.WriteGameObject(Object);
                Writer.WriteOptimized(ZoneID);
                Writer.WriteOptimized(Location.X);
                Writer.WriteOptimized(Location.Y);
            }
            public void Read(SerializationReader Reader)
            {
                _Object = Reader.ReadGameObject();
                ZoneID = Reader.ReadOptimizedString();
                Location = new(
                    X: Reader.ReadOptimizedInt32(),
                    Y: Reader.ReadOptimizedInt32());
            }

            #endregion

            public bool HasObject()
                => GameObject.Validate(Object)
                && Object.CurrentCell != null
                && !Object.InActiveZone()
                && !Object.InSameZone(ParentObject);

            public Cell GetCurrentCellOrCell()
                => !ValidateAlert(ref ParentAlert)
                    || !HasObject()
                    || Object.CurrentCell is not Cell currentCell
                ? Cell
                : currentCell;

            public bool TryGetCurrentCellOrCell(out Cell Cell)
                => (Cell = GetCurrentCellOrCell()) != null;

            public GameObject GetObject()
                => GameObject.Validate(Object)
                ? Object
                : null;

            public bool TryGetObject(out GameObject Object)
                => (Object = GetObject()) != null;
        }

        [Serializable]
        public struct AlertGrammar : IComposite
        {
            public string Verb;
            public string Verbed;
            public string Verbing;

            public AlertGrammar(string Verb, string Verbed, string Verbing)
            {
                this.Verb = Verb;
                this.Verbed = Verbed;
                this.Verbing = Verbing;
            }
        }

        public string Name => GetType().ToStringWithGenerics();

        private AlertGrammar _Grammar;
        public AlertGrammar Grammar
        {
            get => _Grammar;
            protected set => _Grammar = value;
        }

        public IPerception Perception;

        public ISense Sense;

        public Cell Origin;

        public AwarenessLevel Level;

        public bool OverridesCombat;

        [NonSerialized]
        protected AlertSource Source;

        public GameObject SourceObject => Source?.GetObject();
        public Cell SourceCell => Source?.GetCurrentCellOrCell();
        public Cell SourceOriginCell => Source?.Cell;

        public bool IsValid => ValidateAlert(this);

        #region Constructors

        protected IAlert()
        {
            Perception = null;
            Sense = null;
            Origin = null;
            Level = AwarenessLevel.None;
            OverridesCombat = false;
            Source = null;
        }
        protected IAlert(IPerception Perception, ISense Sense, AwarenessLevel Level, bool OverridesCombat, AlertSource Source)
            : this()
        {
            this.Perception = Perception;
            this.Sense = Sense;
            Origin = Perception.Owner.CurrentCell;
            this.Level = Level;
            this.OverridesCombat = OverridesCombat;
            this.Source = Source?.SetParentAlert(this);
        }
        public IAlert(SenseContext Context, ISense Sense, AwarenessLevel Level, bool OverridesCombat)
            : this(Context.Perception, Sense, Level, OverridesCombat, (AlertSource)null)
        {
            Source = new AlertSource(this, Context);
        }
        public IAlert(IAlert Source)
            : this(Source.Perception, Source.Sense, Source.Level, Source.OverridesCombat, Source.Source)
        {
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteComposite(Source);
            Writer.WriteComposite(Grammar);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Source = Reader.ReadComposite<AlertSource>();
            Grammar = Reader.ReadComposite<AlertGrammar>();
        }

        #endregion

        public abstract IAlert Copy();

        protected virtual IAlert<T, TSense> FromSenseContext<T, TSense>(SenseContext Context)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
            => Context != null
            ? SetPerception<T, TSense>(Context.Perception)
                .SetSource<T, TSense>(new AlertSource(this, Context))
            : throw new ArgumentNullException(nameof(Context), "Cannot configure " + Name + " with null " + nameof(SenseContext) + ".");

        protected virtual IAlert<T, TSense> SetPerception<T, TSense>(IPerception Perception)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            this.Perception = Perception
                ?? throw New_ArgNullException(Perception, nameof(Perception));

            Origin = Perception.Owner.CurrentCell;
            return this as IAlert<T, TSense>;
        }
        protected virtual IAlert<T, TSense> SetSense<T, TSense>(ISense Sense)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            this.Sense = Sense
                ?? throw New_ArgNullException(Sense, nameof(Sense));

            return this as IAlert<T, TSense>;
        }
        protected virtual IAlert<T, TSense> SetAwarenessLevel<T, TSense>(AwarenessLevel Level)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (Level == AwarenessLevel.None)
                throw new ArgumentOutOfRangeException(nameof(Level), Name + " requires an " + nameof(AwarenessLevel) + " greater than " + AwarenessLevel.None.ToStringWithNum() + " to function.");

            this.Level = Level;
            return this as IAlert<T, TSense>;
        }
        protected virtual IAlert<T, TSense> SetSource<T, TSense>(AlertSource Source)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            this.Source = Source
                ?? throw New_ArgNullException(Source, nameof(Source));

            return this as IAlert<T, TSense>;
        }
        protected virtual IAlert<T, TSense> SetOverridesCombat<T, TSense>(bool? OverridesCombat)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (OverridesCombat.HasValue)
                this.OverridesCombat = OverridesCombat.GetValueOrDefault();

            return this as IAlert<T, TSense>;
        }
        protected ArgumentNullException New_ArgNullException<T>(T Param, string ParamName)
            => new(ParamName, Name + " requires an " + Param.GetType().ToStringWithGenerics() + " to function.");

        public static bool ValidateAlert(IAlert Alert)
            => Alert != null
            && Alert.ParentBrain is Brain parentBrain
            && parentBrain.ParentObject is GameObject parentObject
            && GameObject.Validate(ref parentObject)
            && Alert.Perception != null
            && Alert.Origin != null;

        public static bool ValidateAlert(ref IAlert Alert)
        {
            if (!ValidateAlert(Alert))
            {
                Alert = null;
                return false;
            }
            return true;
        }

        public override void Create()
        {
            base.Create();
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ParentObject?.MiniDebugName()),
                    Debug.Arg(Perception?.ToString(Short: true, SourceObject)),
                });
        }

        public override bool CanFight()
            => !OverridesCombat;

        public override bool Finished()
            => !IsValid;

        public override void TakeAction()
        {
            if (!IsValid)
                FailToParent();

            base.TakeAction();
        }
    }
}
