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
    public class GetSneakPerformanceEvent : ISneakEvent<GetSneakPerformanceEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetSneakPerformanceEvent()
            : base()
        {
        }

        public static SneakPerformance GetFor(GameObject Hider)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Hider)
                || Process(
                    Hider: Hider,
                    Performance: new(), 
                    Success: out bool success) is not GetSneakPerformanceEvent E
                || !success)
                return null;
;
            if (success)
                success = E.Witnesses.FireEvent(E.StringyEvent, true);

            if (success)
                E.UpdateFromStringyEvent();

            if (success)
                success = E.Witnesses.HandleEvent(E, true);

            return E.Performance;
        }
    }
}

