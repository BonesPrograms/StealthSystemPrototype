using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public abstract class IConcealedAction : Rack<ISense>
    {
        public int MinID;
        public string ID;
        public string Name;

        protected IEvent SourceEvent;

        public string Description;

        protected IConcealedAction()
        {
            MinID = 0;
            ID = null;
            Name = null;

            SourceEvent = null;

            Description = null;
        }
        protected IConcealedAction(int MinID, string Name, string Description)
            : this()
        {
            this.MinID = MinID;
            this.Name = Name;
            ID = Name;

            this.Description = Description;
        }
        protected IConcealedAction(string ID, string Description)
            : this(0, ID, Description)
        {
        }
        protected IConcealedAction(IEvent SourceEvent, int MinID, string Name, string Description)
            : this(MinID, Name, Description)
        {
            this.SourceEvent = SourceEvent;
        }
        protected IConcealedAction(IEvent SourceEvent, string Description)
            : this(SourceEvent, 0, null, Description)
        {
        }
    }
}
