using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Alerts;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedCommandAction : ConcealedMinAction<CommandEvent>
    {
        public string Command;

        public ConcealedCommandAction(CommandEvent E, string Action, bool Aggressive, string Description)
            : base(E, Action,  Aggressive, Description)
        {
            Command = E.Command;
        }
        public ConcealedCommandAction(string Action, bool Aggressive, string Description)
            : base(null, Action,  Aggressive, Description)
        {
        }

        public override ConcealedMinAction<CommandEvent> SetEvent(CommandEvent Event)
        {
            Command = Event.Command;
            return base.SetEvent(Event) as ConcealedCommandAction;
        }
    }
}
