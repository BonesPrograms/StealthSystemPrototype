using System;
using System.Collections.Generic;
using System.Text;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public abstract class IConcealedAction : Rack<ISense>
    {
        public GameObject Actor;

        public int MinID;
        public string ID;
        public string Name;

        protected IEvent SourceEvent;

        public bool Aggressive;

        public string Description;

        protected IConcealedAction()
        {
            Actor = null;

            MinID = 0;
            ID = null;
            Name = null;

            SourceEvent = null;

            Aggressive = false;

            Description = null;
        }
        protected IConcealedAction(int MinID, string Name, bool Aggressive, string Description)
            : this()
        {
            this.MinID = MinID;
            this.Name = Name;
            ID = Name;

            this.Aggressive = Aggressive;

            this.Description = Description;
        }
        protected IConcealedAction(string ID, bool Aggressive, string Description)
            : this(0, ID, Aggressive, Description)
        {
        }
        protected IConcealedAction(IEvent SourceEvent, int MinID, string Name, bool Aggressive, string Description)
            : this(MinID, Name, Aggressive, Description)
        {
            this.SourceEvent = SourceEvent;
        }
        protected IConcealedAction(IEvent SourceEvent, bool Aggressive, string Description)
            : this(SourceEvent, 0, null, Aggressive, Description)
        {
        }

        public IConcealedAction AdjustSenseIntensities(SneakPerformance Performance)
        {
            foreach (ISense sense in this)
                sense.AdjustIntensity(-Performance[sense].Rating);
            return this;
        }
    }
}
