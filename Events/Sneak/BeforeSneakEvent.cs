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
    public class BeforeSneakEvent : ISneakEvent<BeforeSneakEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public BeforeSneakEvent()
            : base()
        {
        }

        public static bool Send(GameObject Hider, SneakPerformance Performance, ref List<GameObject> Witnesses)
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
                    Performance: null, 
                    Witnesses: ref Witnesses,
                    Success: out bool success) is not BeforeSneakEvent E
                || !success
                || E.Witnesses.IsNullOrEmpty())
                return false;
;
            if (success)
                success = E.Witnesses.FireEvent(E.StringyEvent, true);

            if (success)
                E.UpdateFromStringyEvent();

            if (success)
                success = E.Witnesses.HandleEvent(E, true);

            return success;
        }
    }
}

