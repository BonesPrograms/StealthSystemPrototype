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
    public class BeforeDetectedEvent : IDetectionEvent<BeforeDetectedEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Message;

        public BeforeDetectedEvent()
            : base()
        {
            Message = null;
        }

        public override void Reset()
        {
            base.Reset();
            Message = null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameter(nameof(Message), Message)
            ;

        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            Message = StringyEvent.GetStringParameter(nameof(Message));
        }

        public static bool CheckHider(GameObject Hider, ref IOpinionDetection Detection, ref string Message)
        {
            if (HiderFromPool(
                Hider: Hider,
                Detection: Detection) is not BeforeDetectedEvent E)
                return false;

            E.Message = Message;
            E.GetStringyEvent();

            Process(E, out bool success);

            Message = E.Message;

            if (!success)
                return false;

            Detection = E.Detection;
            return true;
        }

        public static bool CheckPerceiver(GameObject Perceiver, ref IOpinionDetection Detection, ref string Message)
        {
            if (PerceiverFromPool(
                Perceiver: Perceiver,
                Detection: Detection) is not BeforeDetectedEvent E)
                return false;

            E.Message = Message;
            E.GetStringyEvent();

            Process(E, out bool success);

            Message = E.Message;

            if (!success)
                return false;

            Detection = E.Detection;
            return true;
        }
    }
}

