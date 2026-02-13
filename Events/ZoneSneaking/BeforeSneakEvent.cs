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
    public class BeforeSneakEvent : IObjectSneakingZoneEvent<BeforeSneakEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Message;

        public BeforeSneakEvent()
            : base()
        {
        }

        public override void Reset()
        {
            base.Reset();
            Message = null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameterOrNullExisting(nameof(Message), Message)
                ;
        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            if (StringyEvent?.GetParameter(nameof(Message)) is string message)
                Message = message;
        }

        public static bool Check(GameObject Sneaker, SneakPerformance Performance, ref string Message)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Sneaker?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Sneaker)
                || FromPool(
                    Zone: Sneaker?.CurrentZone,
                    Sneaker: Sneaker,
                    Performance: Performance) is not BeforeSneakEvent E)
                return false;

            E.Message = Message;
            E.GetStringyEvent();

            ProcessSneaker(E, Success: out bool success);
            Message = E.Message;

            return success;
        }
    }
}

