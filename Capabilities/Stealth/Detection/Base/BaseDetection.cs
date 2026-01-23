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
    public abstract class BaseDetection : GoalHandler, IComposite//, IComparable<BaseAlert>
    {
        [Serializable]
        protected class DetectionSource : IComposite
        {
            public BaseDetection ParentAlert;

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

            public DetectionSource()
            {
                ParentAlert = null;
                _Object = null;
                ZoneID = null;
                Location = default;
            }
            public DetectionSource(BaseDetection ParentAlert, GameObject Object, Cell Cell)
                : this()
            {
                this.ParentAlert = ParentAlert;
                _Object = Object;
                this.Cell = Cell;
            }
            public DetectionSource(BaseDetection ParentAlert, GameObject Object)
                : this(ParentAlert, Object, Object?.CurrentCell)
            {
            }
            public DetectionSource(BaseDetection ParentAlert, Cell Cell)
                : this(ParentAlert, null, Cell)
            {
            }
            public DetectionSource(BaseDetection ParentAlert, AlertContext Context)
                : this(ParentAlert, Context.Perceiver, Context.Perceiver?.CurrentCell)
            {
            }
            public DetectionSource(DetectionSource Source)
                : this(Source.ParentAlert, Source.Object, Source.Cell)
            {
            }
            public DetectionSource(BaseDetection ParentAlert)
                : this(ParentAlert.Source)
            {
                SetParentAlert(ParentAlert);
            }

            public DetectionSource SetParentAlert(BaseDetection Alert)
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
        public struct DetectionGrammar : IComposite
        {
            public string Verb;
            public string Verbed;
            public string Verbing;

            public DetectionGrammar(string Verb, string Verbed, string Verbing)
            {
                this.Verb = Verb;
                this.Verbed = Verbed;
                this.Verbing = Verbing;
            }
        }

        public string Name => GetType().ToStringWithGenerics();

        private DetectionGrammar _Grammar;
        public DetectionGrammar Grammar
        {
            get => _Grammar;
            protected set => _Grammar = value;
        }

        public IPerception Perception;

        public IAlert Alert;

        public Cell Origin;

        public AwarenessLevel Level;

        public bool OverridesCombat;

        [NonSerialized]
        protected DetectionSource Source;

        public GameObject SourceObject => Source?.GetObject();
        public Cell SourceCell => Source?.GetCurrentCellOrCell();
        public Cell SourceOriginCell => Source?.Cell;

        public bool IsValid => ValidateAlert(this);

        #region Constructors

        protected BaseDetection()
        {
            Perception = null;
            Alert = null;
            Origin = null;
            Level = AwarenessLevel.None;
            OverridesCombat = false;
            Source = null;
        }
        protected BaseDetection(IPerception Perception, IAlert Alert, AwarenessLevel Level, bool OverridesCombat, DetectionSource Source)
            : this()
        {
            this.Perception = Perception;
            this.Alert = Alert;
            Origin = Perception.Owner.CurrentCell;
            this.Level = Level;
            this.OverridesCombat = OverridesCombat;
            this.Source = Source?.SetParentAlert(this);
        }
        public BaseDetection(AlertContext Context, IAlert Alert, AwarenessLevel Level, bool OverridesCombat)
            : this(Context.Perception, Alert, Level, OverridesCombat, (DetectionSource)null)
        {
            Source = new DetectionSource(this, Context);
        }
        public BaseDetection(BaseDetection Source)
            : this(Source.Perception, Source.Alert, Source.Level, Source.OverridesCombat, Source.Source)
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
            Source = Reader.ReadComposite<DetectionSource>();
            Grammar = Reader.ReadComposite<DetectionGrammar>();
        }

        #endregion

        public abstract BaseDetection Copy();

        protected virtual Detection<T, TSense> FromAlertContext<T, TSense>(AlertContext Context)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
            => Context != null
            ? SetPerception<T, TSense>(Context.Perception)
                .SetSource<T, TSense>(new DetectionSource(this, Context))
            : throw new ArgumentNullException(nameof(Context), "Cannot configure " + Name + " with null " + nameof(AlertContext) + ".");

        protected virtual Detection<T, TSense> SetPerception<T, TSense>(IPerception Perception)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            this.Perception = Perception
                ?? throw New_ArgNullException(Perception, nameof(Perception));

            Origin = Perception.Owner.CurrentCell;
            return this as Detection<T, TSense>;
        }
        protected virtual Detection<T, TSense> SetSense<T, TSense>(ISense Sense)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            this.Alert = Sense
                ?? throw New_ArgNullException(Sense, nameof(Sense));

            return this as Detection<T, TSense>;
        }
        protected virtual Detection<T, TSense> SetAwarenessLevel<T, TSense>(AwarenessLevel Level)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (Level == AwarenessLevel.None)
                throw new ArgumentOutOfRangeException(nameof(Level), Name + " requires an " + nameof(AwarenessLevel) + " greater than " + AwarenessLevel.None.ToStringWithNum() + " to function.");

            this.Level = Level;
            return this as Detection<T, TSense>;
        }
        protected virtual Detection<T, TSense> SetSource<T, TSense>(DetectionSource Source)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            this.Source = Source
                ?? throw New_ArgNullException(Source, nameof(Source));

            return this as Detection<T, TSense>;
        }
        protected virtual Detection<T, TSense> SetOverridesCombat<T, TSense>(bool? OverridesCombat)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (OverridesCombat.HasValue)
                this.OverridesCombat = OverridesCombat.GetValueOrDefault();

            return this as Detection<T, TSense>;
        }
        protected ArgumentNullException New_ArgNullException<T>(T Param, string ParamName)
            => new(ParamName, Name + " requires an " + Param.GetType().ToStringWithGenerics() + " to function.");

        public static bool ValidateAlert(BaseDetection Alert)
            => Alert != null
            && Alert.ParentBrain is Brain parentBrain
            && parentBrain.ParentObject is GameObject parentObject
            && GameObject.Validate(ref parentObject)
            && Alert.Perception != null
            && Alert.Origin != null;

        public static bool ValidateAlert(ref BaseDetection Alert)
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
