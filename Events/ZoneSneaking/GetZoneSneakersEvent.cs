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
using XRL.World.ZoneParts;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetZoneSneakersEvent : ISneakingZoneEvent<GetZoneSneakersEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetZoneSneakersEvent()
            : base()
        { }

        public static Sneakers GetFor(Zone Zone, ref Sneakers Sneakers)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Zone?.DebugName ?? "null"),
                });

            if (Process(Zone, ref Sneakers, out bool success) is not GetZoneSneakersEvent E
                || !success)
            {
                Sneakers = new(Zone?.RequirePart<UD_SneakWitnesser>());
                return Sneakers;
            }

            return Sneakers = E.Sneakers; Sneakers
        }

        public GetZoneSneakersEvent AddSneaker(ISneakSource SneakSource)
        {
            if (SneakSource != null
                && SneakSource.SneakSneaking)
                SneakWitnesser?.AddSneaker(SneakSource);
            return this;
        }
    }
}

