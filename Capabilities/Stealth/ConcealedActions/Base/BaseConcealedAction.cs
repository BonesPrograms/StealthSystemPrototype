using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using Genkit;

using XRL.World;
using XRL.World.Parts;
using XRL.Collections;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.AlertExtensions;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [StealthSystemBaseClass]
    [Serializable]
    public class BaseConcealedAction
        : AlertRack
        , IConcealedAction
    {
        public override CoalesceMethod DefaultCoalesceMethod => CoalesceMethod.Highest;

        private string _ID;
        public virtual string ID
        {
            get => _ID;
            protected set => _ID = value;
        }

        private string _Name;
        public virtual string Name
        {
            get => _Name;
            protected set => _Name = value;
        }

        private string _Action;
        public virtual string Action
        {
            get => _Action;
            set => _Action = value;
        }

        private GameObject _Hider;
        public virtual GameObject Hider
        {
            get => _Hider;
            protected set => _Hider = value;
        }

        private GameObject _AlertObject;
        public virtual GameObject AlertObject
        {
            get => _AlertObject;
            protected set => _AlertObject = value;
        }

        private Cell _AlertLocation;
        public virtual Cell AlertLocation
        {
            get => _AlertLocation;
            protected set => _AlertLocation = value;
        }

        private SneakPerformance _SneakPerformance;
        public virtual SneakPerformance SneakPerformance => _SneakPerformance ??= Hider?.GetPart<UD_Sneak>()?.SneakPerformance;

        public virtual bool Aggressive { get; } = false;

        public string Description;

        #region Constructors

        public BaseConcealedAction()
        {
            ID = null;
            Name = null;
            Action = null;
            Hider = null;
            AlertObject = null;
            AlertLocation = null;
            Aggressive = false;
            Description = null;
        }
        protected BaseConcealedAction(string ID, string Name, string Action, bool Aggressive, string Description)
            : this()
        {
            this.ID = ID ?? Name;
            this.Name = Name ?? this.ID;
            this.Action = Action ?? this.Name;
            this.Hider = Hider;
            this.Aggressive = Aggressive;
            this.Description = Description;
        }
        protected BaseConcealedAction(string Name, string Action, bool Aggressive, string Description)
            : this(
                  ID: Name,
                  Name: Name,
                  Action: Action,
                  Aggressive: Aggressive,
                  Description: Description)
        {
        }
        public BaseConcealedAction(IConcealedAction Source)
            : this()
        {
            Items = Source.ToArray();
            Length = ((ICollection<BaseAlert>)Source).Count;
            Size = ((ICollection<BaseAlert>)Source).Count;
            ID = Source.GetID();
            Name = Source.GetName();
            Action = Source.GetAction();
            Hider = Source.GetHider();
            AlertObject = Source.GetAlertObject();
            AlertLocation = Source.GetAlertLocation();
            Aggressive = Source.GetAggressive();
            Description = Source.GetDescription();
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteOptimized(ID);
            Writer.WriteOptimized(Name);
            Writer.WriteOptimized(Action);
            Writer.WriteGameObject(Hider);
            Writer.WriteGameObject(AlertObject);
            Writer.Write(AlertLocation);
            Writer.WriteComposite(SneakPerformance);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            ID = Reader.ReadOptimizedString();
            Name = Reader.ReadOptimizedString();
            Action = Reader.ReadOptimizedString();
            Hider = Reader.ReadGameObject();
            AlertObject = Reader.ReadGameObject();
            AlertLocation = Reader.ReadCell();
            _SneakPerformance = Reader.ReadComposite<SneakPerformance>();
        }

        #endregion

        public string GetID()
            => ID;

        public string GetName()
            => Name;

        public string GetAction()
            => Action;

        public GameObject GetHider()
            => Hider;

        public GameObject GetAlertObject()
            => AlertObject;

        public Cell GetAlertLocation()
            => AlertLocation;

        public SneakPerformance GetSneakPerformance()
            => SneakPerformance;

        public bool GetAggressive()
            => Aggressive;

        public string GetDescription()
            => Description;

        public virtual BaseConcealedAction Initialize()
            => Coalesce() as BaseConcealedAction;

        IConcealedAction IConcealedAction.Initialize()
            => Initialize();

        public virtual BaseConcealedAction SetHider(GameObject Hider)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.MiniDebugName() ?? "null"),
                });

            this.Hider = Hider;
            return this;
        }

        IConcealedAction IConcealedAction.SetHider(GameObject Hider)
            => SetHider(Hider);

        public virtual BaseConcealedAction SetAlertObject(GameObject AlertObject = null)
        {
            this.AlertObject = AlertObject ?? Hider;

            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(AlertObject?.MiniDebugName() ?? "null"),
                    Debug.Arg("set", this.AlertObject?.MiniDebugName() ?? "null"),
                });
            return this;
        }

        IConcealedAction IConcealedAction.SetAlertObject(GameObject AlertObject)
            => SetAlertObject(AlertObject);

        public virtual BaseConcealedAction SetAlertLocation(Cell AlertLocation = null)
        {
            this.AlertLocation = AlertLocation ?? Hider?.CurrentCell;

            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg("[" + (AlertLocation?.Location ?? new Location2D(0,0)) + "]"),
                    Debug.Arg("set", "[" + (this.AlertLocation?.Location ?? new Location2D(0,0)) + "]"),
                });
            return this;
        }

        IConcealedAction IConcealedAction.SetAlertLocation(Cell AlertLocation)
            => SetAlertLocation(AlertLocation);


        public virtual void Configure()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(ID, Name),
                    Debug.Arg(nameof(Count), Count),
                });
        }

        public void ReplaceActionAlerts(IEnumerable<BaseAlert> NewActionAlerts)
        {
            Clear();
            if (!NewActionAlerts.IsNullOrEmpty())
                AddRange(NewActionAlerts);
        }
    }
}
