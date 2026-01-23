using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class AfterAlertEvent : IAlertEvent<AfterAlertEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public AfterAlertEvent()
        {
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent();

        public override void UpdateFromStringyEvent()
            =>base.UpdateFromStringyEvent();

        public static void Send<T, TSense>(GameObject Perceiver, GameObject Hider, Detection<T, TSense> Alert)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (FromPool(
                    Perceiver: Perceiver,
                    Hider: Hider,
                    Alert: Alert) is not AfterAlertEvent E)
                return;

            Process(E, out bool success);

            if (!success)
                return;

            ZoneProcess(E, out _);
        }
    }
}

