using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Senses;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ConcealedCommandAction : ConcealedMinAction<CommandEvent>
    {
        public string Command;

        public ConcealedCommandAction(CommandEvent E, bool Aggressive, string Description)
            : base(E, Aggressive, Description)
        {
            Command = E.Command;
        }
        public ConcealedCommandAction(bool Aggressive, string Description)
            : base(Aggressive, Description)
        {
        }

        public override ConcealedMinAction<CommandEvent> SetEvent(CommandEvent E)
        {
            Command = E.Command;
            return base.SetEvent(E);
        }
    }
}
