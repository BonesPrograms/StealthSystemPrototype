using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Sneak;
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

        public static bool CheckHider<TSense, TAlert>(GameObject Hider, ref TAlert Alert)
            where TSense : ISense<TSense>, new()
            where TAlert : IAlert<IPerception<TSense>, TSense>
        {
            if (Process(
                E: HiderFromPool(
                    Hider: Hider,
                    Alert: Alert),
                Success: out bool success) is not BeforeAlertEvent E
                || !success)
                return false;

            Alert = E.Alert as TAlert;
            return true;
        }

        public static bool CheckPerceiver<TSense, TAlert>(GameObject Perceiver, ref TAlert Alert)
            where TSense : ISense<TSense>, new()
            where TAlert : IAlert<IPerception<TSense>, TSense>
        {
            if (Process(
                E: PerceiverFromPool(
                    Perceiver: Perceiver,
                    Alert: Alert),
                Success: out bool success) is not BeforeAlertEvent E
                || !success)
                return false;

            Alert = E.Alert as TAlert;
            return true;
        }
    }
}

