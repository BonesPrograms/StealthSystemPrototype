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
using XRL.Collections;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetSneakDetailsEvent : ISneakEvent<GetSneakDetailsEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        protected StringMap<string> DetailsEntries;

        public GetSneakDetailsEvent()
            : base()
        {
            DetailsEntries = null;
        }

        public override void Reset()
        {
            base.Reset();
            DetailsEntries = null;
        }

        public static StringMap<string> GetFor(GameObject Hider)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Hider)
                || FromPool(Hider) is not GetSneakDetailsEvent E)
                return null;

            E.DetailsEntries = new();

            return Process(E, Success: out bool success).DetailsEntries;
        }

        public GetSneakDetailsEvent Add<T>(T Source, string Details)
            where T : IComponent<GameObject>, new()
        {
            if (Source != null)
                DetailsEntries[Source?.TypeStringWithGenerics()] = Details;
            return this;
        }
    }
}

