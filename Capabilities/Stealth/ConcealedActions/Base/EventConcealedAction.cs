using System;
using System.Collections.Generic;
using System.Text;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Alerts;

using XRL.Collections;
using XRL.World;
using System.Linq;
using XRL.World.Parts;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [StealthSystemBaseClass]
    public abstract class EventConcealedAction<E>
        : BaseConcealedAction
        where E : class, IEvent
    {
        protected E SourceEvent;

        public override string Name => base.Name ??= SourceEvent?.GetType()?.ToStringWithGenerics();

        public EventConcealedAction()
            : base()
        {
            SourceEvent = null;
        }
        public EventConcealedAction(string ID, E SourceEvent, bool Aggressive, string Description)
            : base(ID, null, Aggressive, Description)
        {
            this.SourceEvent = SourceEvent;
        }
        public EventConcealedAction(E Source, bool Aggressive, string Description)
            : this(null, Source, Aggressive, Description)
        {
            ID = Name;
        }
    }
}
