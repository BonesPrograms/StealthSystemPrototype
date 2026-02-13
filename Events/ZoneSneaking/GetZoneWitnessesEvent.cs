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
    public class GetZoneWitnessesEvent : ISneakingZoneEvent<GetZoneWitnessesEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetZoneWitnessesEvent()
            : base()
        { }

        public static Witnesses GetFor(Zone Zone, ref Witnesses Witnesses)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Zone?.DebugName ?? "null"),
                });

            if (Process(Zone, ref Witnesses, out bool success) is not GetZoneWitnessesEvent E
                || !success)
            {
                Witnesses = new(Zone?.RequirePart<UD_SneakWitnesser>());
                return Witnesses;
            }

            return Witnesses = E.Witnesses;
        }

        public GetZoneWitnessesEvent AddWitness(GameObject Witness)
        {
            SneakWitnesser?.AddWitness(Witness);
            return this;
        }

        public GetZoneWitnessesEvent AddWitness(IComponent<GameObject> WitnessComponent)
            => AddWitness(WitnessComponent?.GetComponentBasis());

        public GetZoneWitnessesEvent AddWitness(BasePerception Perception)
            => AddWitness(Perception?.Owner);
    }
}

