using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedMinAction<T> : IConcealedAction
        where T : MinEvent, new()
    {
        public T Event => SourceEvent as T;

        public ConcealedMinAction(T SourceEvent, string Description)
            : base(SourceEvent, SourceEvent?.ID ?? 0, SourceEvent?.TypeStringWithGenerics(), Description)
        {
        }
    }
}
