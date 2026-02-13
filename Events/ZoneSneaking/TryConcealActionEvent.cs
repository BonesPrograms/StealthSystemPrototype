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
    [GameEvent(Cascade = CASCADE_STOP_AT_ZONE | CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class TryConcealActionEvent : IObjectSneakingZoneEvent<TryConcealActionEvent>
    {
        public BaseConcealedAction ConcealedAction;

        public TryConcealActionEvent()
            : base()
        { }

        public override void Reset()
        {
            base.Reset();
            ConcealedAction = null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                .SetParameterOrNullExisting(nameof(ConcealedAction), ConcealedAction)
                ;

        public static void Send(GameObject Sneaker, SneakPerformance Performance, BaseConcealedAction ConcealedAction)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Sneaker?.MiniDebugName() ?? "null"),
                });

            if (FromPool(Sneaker, Performance: Performance) is not TryConcealActionEvent E)
                return;

            E.ConcealedAction = ConcealedAction;

            E.GetStringyEvent();

            Process(E, Success: out bool success);
            Debug.YehNah(nameof(ProcessSneaker), success, Indent: indent[1]);

            ZoneProcess(E, out success);
            Debug.YehNah(nameof(ZoneProcess), success, Indent: indent[1]);
        }
    }
}

