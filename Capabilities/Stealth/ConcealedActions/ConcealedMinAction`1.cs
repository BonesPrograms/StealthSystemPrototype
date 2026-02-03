using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedMinAction<T> : EventConcealedAction<T>
        where T : MinEvent, new()
    {
        public T Event => SourceEvent;

        public int MinID => Event?.ID ?? 0;

        public ConcealedMinAction(T E, string Action, bool Aggressive, string Description)
            : base(
                  ID: E?.TypeStringWithGenerics(),
                  SourceEvent: E,
                  Action: Action,
                  Aggressive: Aggressive,
                  Description: Description)
        {
        }

        public virtual ConcealedMinAction<T> SetEvent(T Event)
        {
            SourceEvent = Event;
            ID = Event?.TypeStringWithGenerics();
            Name = ID;
            return this;
        }
    }
}
