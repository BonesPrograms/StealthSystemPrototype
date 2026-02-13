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
using XRL.World.ZoneParts;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_STOP_AT_ZONE | CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IObjectSneakingZoneEvent<T> : ISneakingZoneEvent<T>
        where T : IObjectSneakingZoneEvent<T>, new()
    {
        public new static readonly int CascadeLevel =
            CASCADE_STOP_AT_ZONE // These are zone events but we don't them sent to every object in the zone.
            | CASCADE_EQUIPMENT // We manually send these events to objects in one of the two lists
            | CASCADE_INVENTORY // which is why we cascade them.
            | CASCADE_SLOTS;

        public GameObject Sneaker;

        public SneakPerformance SneakPerformance;

        public IObjectSneakingZoneEvent()
            : base()
        {
            Sneaker = null;
            SneakPerformance = null;
        }

        public override void Reset()
        {
            base.Reset();
            Sneaker = null;
            SneakPerformance = null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                .SetParameterOrNullExisting(nameof(Sneaker), Sneaker)
                .SetParameterOrNullExisting(nameof(SneakPerformance), SneakPerformance)
            ;

        public static T FromPool(Zone Zone, GameObject Sneaker, SneakPerformance Performance)
        {
            if (FromPool(Zone, out UD_SneakWitnesser sneakWitnesser) is not T E)
                return null;
            E.Witnesses = sneakWitnesser.Witnesses;
            E.Sneakers = sneakWitnesser.Sneakers;
            E.Sneaker = Sneaker;
            E.SneakPerformance = Performance;
            E.GetStringyEvent();
            return E;
        }

        public static T ProcessSneaker(T E, out bool Success)
        {
            if (E == null)
            {
                Success = false;
                return null;
            }
            Success = true;
            if (GameObject.Validate(ref E.Sneaker))
            {
                if (Success
                    && E.Sneaker.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = E.Sneaker.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && E.Sneaker.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Sneaker.HandleEvent(E);
            }
            if (Success)
                WitnessesProcess(E, out Success);

            return E;
        }

        public static void Send(GameObject Sneaker, SneakPerformance Performance)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Sneaker?.DebugName ?? "null"),
                    Debug.Arg(Sneaker?.CurrentZone?.DebugName ?? "null"),
                });

            if (ProcessSneaker(
                E: FromPool(
                    Zone: Sneaker?.CurrentZone,
                    Sneaker: Sneaker,
                    Performance: Performance),
                Success: out bool _) is not T E)
                return;
        }
    }
}

