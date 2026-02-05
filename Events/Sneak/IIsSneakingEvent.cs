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

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IIsSneakingEvent<T> : ISneakEvent<T>
        where T : IIsSneakingEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public IIsSneakingEvent()
            : base()
        {
        }

        public static void Send(GameObject Hider, SneakPerformance Performance, List<GameObject> Witnesses)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Hider)
                || FromPool(
                    Hider: Hider,
                    Performance: Performance,
                    Witnesses: ref Witnesses,
                    CollectWitnesses: false) is not T E)
                return;

            E.GetStringyEvent();

            Process(E, Success: out bool _);

            ZoneProcess(E, out bool _);
        }
    }
}

