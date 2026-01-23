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
    public class BeforeAlertEvent : IAlertEvent<BeforeAlertEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public BeforeAlertEvent()
        {
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent();

        public override void UpdateFromStringyEvent()
            =>base.UpdateFromStringyEvent();

        public static bool CheckHider<T, TSense>(GameObject Hider, ref Detection<T, TSense> Alert)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (Process(
                E: HiderFromPool(
                    Hider: Hider,
                    Alert: Alert),
                Success: out bool success) is not BeforeAlertEvent E
                || !success)
                return false;

            Alert = E.Alert as Detection<T, TSense>;
            return true;
        }

        public static bool CheckPerceiver<T, TSense>(GameObject Perceiver, ref Detection<T, TSense> Alert)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (Process(
                E: PerceiverFromPool(
                    Perceiver: Perceiver,
                    Alert: Alert),
                Success: out bool success) is not BeforeAlertEvent E
                || !success)
                return false;

            Alert = E.Alert as Detection<T, TSense>;
            return true;
        }
    }
}

