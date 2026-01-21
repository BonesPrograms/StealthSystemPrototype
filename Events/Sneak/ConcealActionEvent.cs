using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class ConcealActionEvent : ISneakEvent<ConcealActionEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public IConcealedAction ConcealedAction;

        public ConcealActionEvent()
            : base()
        {
        }

        public static void Send(GameObject Hider, SneakPerformance Performance, IConcealedAction ConcealedAction)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Hider)
                || FromPool(Hider, Performance: ref Performance) is not ConcealActionEvent E)
                return;

            E.ConcealedAction = ConcealedAction;

            Process(E, Success: out bool success);

            if (success)
                success = E.Hider.GetCurrentZone().FireEvent(E.StringyEvent);

            if (success)
                E.UpdateFromStringyEvent();

            if (success)
                success = E.Hider.GetCurrentZone().HandleEvent(E);

        }
    }
}

