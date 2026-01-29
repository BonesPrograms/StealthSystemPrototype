using System;
using System.Collections.Generic;
using System.Text;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Alerts;

using XRL.Collections;
using XRL.World;
using System.Linq;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public abstract class BaseConcealedAction : Rack<IAlert>, IConcealedAction
    {
        protected string _ID;
        public string ID
        {
            get => _ID;
            protected set => _ID = value;
        }

        protected string _Name;
        public string Name
        {
            get => _Name;
            protected set => _Name = value;
        }

        protected string _Action;
        public virtual string Action
        {
            get => _Action;
            protected set => _Action = value;
        }

        protected GameObject _Actor;
        public GameObject Actor
        {
            get => _Actor;
            protected set => _Actor = value;
        }

        protected GameObject _AlertObject;
        public virtual GameObject AlertObject
        {
            get => _AlertObject;
            protected set => _AlertObject = value;
        }

        protected Cell _AlertLocation;
        public virtual Cell AlertLocation
        {
            get => _AlertLocation;
            protected set => _AlertLocation = value;
        }

        protected SneakPerformance _SneakPerformance;
        public virtual SneakPerformance SneakPerformance
        {
            get => _SneakPerformance;
            protected set => _SneakPerformance = value;
        }

        protected bool _Aggressive;
        public virtual bool Aggressive
        {
            get => _Aggressive;
            protected set => _Aggressive = value;
        }

        public int MinID;

        protected IEvent SourceEvent;

        public string Description;

        protected BaseConcealedAction()
        {

            MinID = 0;
            ID = null;
            Name = null;
            Action = null;

            Actor = null;
            AlertObject = null;
            AlertLocation = null;

            SourceEvent = null;

            Aggressive = false;

            Description = null;
        }
        protected BaseConcealedAction(int MinID, string Name, bool Aggressive, string Description)
            : this()
        {
            this.MinID = MinID;
            this.Name = Name;
            ID = Name;

            this.Aggressive = Aggressive;

            this.Description = Description;
        }
        protected BaseConcealedAction(string ID, bool Aggressive, string Description)
            : this(0, ID, Aggressive, Description)
        {
        }
        protected BaseConcealedAction(IEvent SourceEvent, int MinID, string Name, bool Aggressive, string Description)
            : this(MinID, Name, Aggressive, Description)
        {
            this.SourceEvent = SourceEvent;
        }
        protected BaseConcealedAction(IEvent SourceEvent, bool Aggressive, string Description)
            : this(SourceEvent, 0, null, Aggressive, Description)
        {
        }

        /*
        public BaseConcealedAction AdjustSenseIntensities(SneakPerformance Performance)
        {
            foreach (IAlert alert in this)
                alert.AdjustIntensity(-Performance[alert]);
            return this;
        }
        */
    }
}
