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

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public abstract class BaseAlert : GoalHandler//, IComparable<BaseAlert>
    {
        [Serializable]
        protected class AlertSource : IComposite
        {
            public BaseAlert ParentAlert;

            private GameObject _Object;
            public GameObject Object => _Object;

            private Cell _Cell;
            public Cell Cell
                => _Cell == null
                    && The.ZoneManager is ZoneManager zoneManager
                    && zoneManager.IsZoneLive(ZoneID)
                ? _Cell = zoneManager.GetZone(ZoneID).GetCell(Location)
                : _Cell;

            private string ZoneID;
            private Location2D Location;

            private Brain ParentBrain => ParentAlert?.ParentBrain;
            private GameObject ParentObject => ParentBrain?.ParentObject;

            private AlertSource()
            {
                ParentAlert = null;
                _Object = null;
                ZoneID = null;
                Location = default;
            }
            public AlertSource(BaseAlert ParentAlert, GameObject Object, Cell Cell)
                : this()
            {
                this.ParentAlert = ParentAlert;
                _Object = Object;
                ZoneID = Cell?.ParentZone?.ZoneID;
                Location = Cell?.Location ?? default;
            }
            public AlertSource(BaseAlert ParentAlert, GameObject Object)
                : this(ParentAlert, Object, Object?.CurrentCell)
            {
            }
            public AlertSource(BaseAlert ParentAlert, Cell Cell)
                : this(ParentAlert, null, Cell)
            {
            }

            public AlertSource SetParentAlert(BaseAlert Alert)
            {
                ParentAlert = Alert;
                return this;
            }

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
        }

        public IPerception Perception;

        public PerceptionSense Sense;

        public Cell Origin;

        private AlertSource Source;

        public GameObject SourceObject => Source?.GetObject();
        public Cell SourceCell => Source?.GetCurrentCellOrCell();
        public Cell SourceOriginCell => Source?.Cell;

        public bool IsValid => ValidateAlert(this);

        #region Constructors

        protected BaseAlert()
        {
            Perception = null;
            Sense = PerceptionSense.None;
            Origin = null;
            Source = null;
        }
        protected BaseAlert(IPerception Perception, AlertSource Source)
            : this()
        {
            this.Perception = Perception;
            Sense = Perception.Sense;
            Origin = Perception.Owner.CurrentCell;
            this.Source = Source?.SetParentAlert(this);
        }
        public BaseAlert(IPerception Perception, Cell SourceCell)
            : this(Perception, (AlertSource)null)
        {
            Source = new AlertSource(this, SourceCell);
        }
        public BaseAlert(IPerception Perception, GameObject SourceObject)
            : this(Perception, (AlertSource)null)
        {
            Source = new AlertSource(this, SourceObject);
        }

        #endregion

        public static bool ValidateAlert(BaseAlert Alert)
            => Alert != null
            && Alert.ParentBrain is Brain parentBrain
            && parentBrain.ParentObject is GameObject parentObject
            && GameObject.Validate(ref parentObject)
            && Alert.Perception != null
            && Alert.Origin != null;

        public static bool ValidateAlert(ref BaseAlert Alert)
        {
            if (!ValidateAlert(Alert))
            {
                Alert = null;
                return false;
            }
            return true;
        }
    }
}
