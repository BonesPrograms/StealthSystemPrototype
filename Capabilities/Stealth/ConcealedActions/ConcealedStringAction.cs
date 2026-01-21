using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedStringAction : IConcealedAction
    {
        public Event Event => SourceEvent as Event;

        public ConcealedStringAction(Event SourceEvent, string Description)
            : base(SourceEvent, 0, SourceEvent.ID, Description)
        {
        }
    }
}
