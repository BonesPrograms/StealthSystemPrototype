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

        private BaseConcealedAction _ParentAction;
        public BaseConcealedAction ParentAction
        { 
            get => _ParentAction;
            protected set => _ParentAction = value;
        }

        private BasePerception _Perception;
        public BasePerception Perception
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

        private BaseAlert _ActionAlert;
        public BaseAlert ActionAlert
        {
            get => _ActionAlert;
            protected set => _ActionAlert = value;
        }

        private BaseAlert _SneakAlert;
        public BaseAlert SneakAlert
        {
            get => _SneakAlert;
            protected set => _SneakAlert = value;
        }

        private GameObject _Hider;
        public GameObject Hider
        {
            get => _Hider;
            protected set => _Hider = value;
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
            ActionAlert = null;
            SneakAlert = null;
            Hider = null;
            AlertObject = null;
            AlertLocation = null;
        }
        protected AlertContext(
            BaseConcealedAction ParentAction,
            BasePerception Perception,
            BaseAlert ActionAlert,
            BaseAlert SneakAlert,
            GameObject Hider,
            GameObject AlertObject,
            Cell AlertLocation)
            : this()
        {
            this.ParentAction = ParentAction;
            this.Perception = Perception;
            this.ActionAlert = ActionAlert;
            this.SneakAlert = SneakAlert;
            this.Hider = Hider;
            this.AlertObject = AlertObject;
            this.AlertLocation = AlertLocation;
        }
        public AlertContext(
            BaseConcealedAction ParentAction,
            BaseAlert ActionAlert,
            BaseAlert SneakAlert,
            GameObject Hider,
            GameObject AlertObject,
            Cell AlertLocation)
            : this(
                  ParentAction: ParentAction,
                  Perception: null,
                  ActionAlert: ActionAlert,
                  SneakAlert: SneakAlert,
                  Hider: Hider,
                  AlertObject: AlertObject,
                  AlertLocation: AlertLocation)
        {
        }
        public AlertContext(AlertContext Source)
            : this(
                  ParentAction: Source.ParentAction,
                  Perception: Source.Perception,
                  ActionAlert: Source.ActionAlert.Copy(Degrade: true),
                  SneakAlert: Source.SneakAlert.Copy(),
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
            ConcealedActionData concealedActionData = (ConcealedActionData)ParentAction;
            Writer.WriteComposite(concealedActionData);
            Writer.WriteComposite(Perception);
            Writer.WriteGameObject(Perceiver);
            Writer.WriteComposite(ActionAlert);
            Writer.WriteComposite(SneakAlert);
            Writer.WriteGameObject(Hider);
            Writer.WriteGameObject(AlertObject);
            Writer.Write(AlertLocation);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ParentAction = (BaseConcealedAction)Reader.ReadComposite<ConcealedActionData>();
            Perception = Reader.ReadComposite<BasePerception>();
            Perceiver = Reader.ReadGameObject();
            ActionAlert = Reader.ReadComposite<BaseAlert>();
            SneakAlert = Reader.ReadComposite<BaseAlert>();
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
            && (Perception == null || Perception.Validate())
            && SneakAlert.Intensity > 0;

        public static bool Validate(ref AlertContext Context)
        {
            if (!Context.Validate())
                Context = null;

            return Context != null;
        }

        public void SetPerception(BasePerception Perception)
        {
            this.Perception = Perception;
        }
    }
}
