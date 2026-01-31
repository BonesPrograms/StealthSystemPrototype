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
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class TryConcealActionEvent : ISneakEvent<TryConcealActionEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public BaseConcealedAction ConcealedAction;

        public TryConcealActionEvent()
            : base()
        {
        }

        public override void Reset()
        {
            base.Reset();
            ConcealedAction = null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                .SetParameterOrNullExisting(nameof(ConcealedAction), ConcealedAction)
                ;

        public static void Send(GameObject Hider, SneakPerformance Performance, BaseConcealedAction ConcealedAction)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Hider)
                || FromPool(Hider, Performance: ref Performance) is not TryConcealActionEvent E)
                return;

            E.ConcealedAction = ConcealedAction;

            E.GetStringyEvent();

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

