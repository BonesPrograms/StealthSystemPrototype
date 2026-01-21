using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedCommandEvent : ConcealedMinAction<CommandEvent>
    {
        public string Command;

        public ConcealedCommandEvent(CommandEvent SourceEvent, string Description)
            : base(SourceEvent, Description)
        {
            Command = SourceEvent.Command;
        }
    }
}
