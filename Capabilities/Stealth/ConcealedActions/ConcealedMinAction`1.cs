using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedMinAction<T> : BaseConcealedAction
        where T : MinEvent, new()
    {
        public T Event => SourceEvent as T;

        public ConcealedMinAction(T E, bool Aggressive, string Description)
            : base(
                  SourceEvent: E,
                  MinID: E?.ID ?? 0,
                  Name: E?.TypeStringWithGenerics(),
                  Aggressive: Aggressive,
                  Description: Description)
        {
        }
        public ConcealedMinAction(bool Aggressive, string Description)
            : base(null, 0, null, Aggressive, Description)
        {
        }

        public virtual ConcealedMinAction<T> SetEvent(T E)
        {
            this.SourceEvent = E;
            MinID = E?.ID ?? 0;
            ID = E?.TypeStringWithGenerics();
            Name = ID;
            return this;
        }
    }
}
