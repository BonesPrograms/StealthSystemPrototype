using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Alerts;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedStringAction : EventConcealedAction<Event>
    {
        public Event Event => SourceEvent;

        public ConcealedStringAction(Event SourceEvent, bool Aggressive, string Description)
            : base(SourceEvent.ID, SourceEvent, Aggressive, Description)
        {
        }
    }
}
