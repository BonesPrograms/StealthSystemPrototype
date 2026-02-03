using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedStringAction : EventConcealedAction<Event>
    {
        public Event Event => SourceEvent;

        public ConcealedStringAction(Event SourceEvent, string Action, bool Aggressive, string Description)
            : base(SourceEvent.ID, SourceEvent, Action, Aggressive, Description)
        {
        }
    }
}
