using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class AfterDetectedEvent : IDetectionEvent<AfterDetectedEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public AfterDetectedEvent()
            : base()
        {
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent();

        public override void UpdateFromStringyEvent()
            =>base.UpdateFromStringyEvent();

        public static void Send(GameObject Perceiver, GameObject Hider, IOpinionDetection Detection)
        {
            if (FromPool(
                    Perceiver: Perceiver,
                    Hider: Hider,
                    Detection: Detection) is not AfterDetectedEvent E)
                return;

            Process(E, out bool success);

            if (!success)
                return;

            ZoneProcess(E, out _);
        }
    }
}

