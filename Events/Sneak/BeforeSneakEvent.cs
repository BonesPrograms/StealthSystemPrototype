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

        public static bool Check(GameObject Hider, SneakPerformance Performance, ref List<GameObject> Witnesses, ref string Message)
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
                    CollectWitnesses: true) is not BeforeSneakEvent E)
                return false;

            E.Message = Message;
            E.GetStringyEvent();

            Process(E, Success: out bool success);
            if (!success)
            {
                Message = E.Message;
                return false;
            }
            else
            if (!E.Witnesses.IsNullOrEmpty())
            {
                if (success)
                    success = E.Witnesses.FireEvent(E.StringyEvent, true);

                if (success)
                    E.UpdateFromStringyEvent();

                if (success)
                    success = E.Witnesses.HandleEvent(E, true);
            }
            Message = E.Message;
            return success;
        }
    }
}

