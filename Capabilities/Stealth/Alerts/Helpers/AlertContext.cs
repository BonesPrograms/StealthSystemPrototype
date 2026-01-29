using System;
using System.Reflection;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;
using XRL.World.Parts;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Alerts
{
    public class AlertContext : IComposite
    {
        public bool WantFieldReflection => false;

        private IConcealedAction _ParentAction;
        public IConcealedAction ParentAction
        { 
            get => _ParentAction;
            protected set => _ParentAction = value;
        }

        private IPerception _Perception;
        public IPerception Perception
        {
            get => _Perception;
            protected set => _Perception = value;
        }

        private GameObject _Perceiver;
        public GameObject Perceiver
        {
            get => _Perceiver;
            protected set => _Perceiver = value;
        }

        private IAlert _Alert;
        public IAlert Alert
        {
            get => _Alert;
            protected set => _Alert = value;
        }

        private int _ActionIntensity;
        public int AlertConcealment
        {
            get => _ActionIntensity;
            protected set => _ActionIntensity = value;
        }

        private GameObject _Actor;
        public GameObject Hider
        {
            get => _Actor;
            protected set => _Actor = value;
        }

        private GameObject _AlertObject;
        public GameObject AlertObject
        {
            get => _AlertObject;
            protected set => _AlertObject = value;
        }

        private Cell _AlertLocation;
        public Cell AlertLocation
        {
            get => _AlertLocation;
            protected set => _AlertLocation = value;
        }

        #region Constructors

        protected AlertContext()
        {
            ParentAction = null;
            Perception = null;
            Alert = null;
            AlertConcealment = 0;
            Hider = null;
            AlertObject = null;
            AlertLocation = null;
        }
        public AlertContext(
            IConcealedAction ParentAction,
            IPerception Perception,
            IAlert Alert,
            int AlertConcealment,
            GameObject Hider,
            GameObject AlertObject,
            Cell AlertLocation)
            : this()
        {
            this.ParentAction = ParentAction;
            this.Perception = Perception;
            this.Alert = Alert;
            this.AlertConcealment = AlertConcealment;
            this.Hider = Hider;
            this.AlertObject = AlertObject;
            this.AlertLocation = AlertLocation;
        }
        public AlertContext(AlertContext Source)
            : this(
                  ParentAction: Source.ParentAction,
                  Perception: Source.Perception,
                  Alert: Source.Alert,
                  AlertConcealment: Source.AlertConcealment - 1,
                  Hider: Source.Hider,
                  AlertObject: Source.AlertObject,
                  AlertLocation:Source.AlertLocation)
        {
        }
        public AlertContext(SerializationReader Reader)
            : this()
        {
            Read(Reader);
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(ParentAction);
            Writer.Write(Perception);
            Writer.WriteGameObject(Perceiver);
            Writer.Write(Alert);
            Writer.WriteOptimized(AlertConcealment);
            Writer.WriteGameObject(Hider);
            Writer.WriteGameObject(AlertObject);
            Writer.Write(AlertLocation);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ParentAction = Reader.ReadComposite() as IConcealedAction;
            Perception = Reader.ReadComposite() as IPerception;
            Perceiver = Reader.ReadGameObject();
            Alert = Reader.ReadComposite() as IAlert;
            AlertConcealment = Reader.ReadOptimizedInt32();
            Hider = Reader.ReadGameObject();
            AlertObject = Reader.ReadGameObject();
            AlertLocation = Reader.ReadCell();
        }

        #endregion

        public virtual AlertContext DeepCopy(GameObject Perceiver)
        {
            AlertContext alertContext = Activator.CreateInstance(GetType()) as AlertContext;

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(alertContext, fieldInfo.GetValue(this));

            alertContext.Perceiver = Perceiver;

            return alertContext;
        }

        public bool Validate()
            => GameObject.Validate(Hider)
            && GameObject.Validate(Perceiver)
            && Perception.Validate()
            && AlertConcealment > 0;

        public static bool Validate(ref AlertContext Context)
        {
            if (!Context.Validate())
                Context = null;

            return Context != null;
        }
    }
}
