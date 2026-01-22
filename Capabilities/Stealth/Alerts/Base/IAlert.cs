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
    public abstract class IAlert : GoalHandler//, IComparable<BaseAlert>
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

            private AlertSource()
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

        public IPerception Perception;

        public ISense Sense;

        public Cell Origin;

        public AwarenessLevel Level;

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
            Source = null;
        }
        protected IAlert(IPerception Perception, ISense Sense, AwarenessLevel Level, AlertSource Source)
            : this()
        {
            this.Perception = Perception;
            this.Sense = Sense;
            Origin = Perception.Owner.CurrentCell;
            this.Level = Level;
            this.Source = Source?.SetParentAlert(this);
        }
        public IAlert(IPerception Perception, ISense Sense, AwarenessLevel Level, Cell SourceCell)
            : this(Perception, Sense, Level, (AlertSource)null)
        {
            Source = new AlertSource(this, SourceCell);
        }
        public IAlert(IPerception Perception, ISense Sense, AwarenessLevel Level, GameObject SourceObject)
            : this(Perception, Sense, Level, (AlertSource)null)
        {
            Source = new AlertSource(this, SourceObject);
        }
        public IAlert(SenseContext Context, ISense Sense, AwarenessLevel Level)
            : this(Context.Perception, Sense, Level, (AlertSource)null)
        {
            Source = new AlertSource(this, Context);
        }
        public IAlert(IAlert Source)
            : this(Source.Perception, Source.Sense, Source.Level, Source.Source)
        {
        }

        #endregion

        public abstract IAlert Copy();

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
    }
}
