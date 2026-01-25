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
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Detetections
{
    [Serializable]
    public class DetectionSource : IComposite
    {
        public IDetection ParentDetection;

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

        private Brain ParentBrain => ParentDetection?.ParentBrain;
        private GameObject ParentObject => ParentBrain?.ParentObject;

        #region Constructors

        public DetectionSource()
        {
            ParentDetection = null;
            _Object = null;
            ZoneID = null;
            Location = default;
        }
        public DetectionSource(IDetection ParentAlert, GameObject Object, Cell Cell)
            : this()
        {
            this.ParentDetection = ParentAlert;
            _Object = Object;
            this.Cell = Cell;
        }
        public DetectionSource(IDetection ParentAlert, GameObject Object)
            : this(ParentAlert, Object, Object?.CurrentCell)
        {
        }
        public DetectionSource(IDetection ParentAlert, Cell Cell)
            : this(ParentAlert, null, Cell)
        {
        }
        public DetectionSource(IDetection ParentAlert, AlertContext Context)
            : this(ParentAlert, Context.Perceiver, Context.Perceiver?.CurrentCell)
        {
        }
        public DetectionSource(DetectionSource Source)
            : this(Source.ParentDetection, Source.Object, Source.Cell)
        {
        }
        public DetectionSource(IDetection ParentAlert)
            : this(ParentAlert.Source)
        {
            SetParentDetection(ParentAlert);
        }

        public DetectionSource SetParentDetection(IDetection Detection)
        {
            ParentDetection = Detection;
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
            => !IDetection.ValidateDetection(ref ParentDetection)
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
}
