using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetDetectionOpinionEvent : IDetectionEvent<GetDetectionOpinionEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public IOpinionDetection ReplacementDetection;

        public AwarenessLevel Level;

        public GetDetectionOpinionEvent()
            : base()
        {
            ReplacementDetection = null;
            Level = AwarenessLevel.None;
        }

        public override void Reset()
        {
            base.Reset();
            ReplacementDetection = null;
            Level = AwarenessLevel.None;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameter(nameof(ReplacementDetection), ReplacementDetection)
            ;

        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            ReplacementDetection = StringyEvent.GetParameter<IOpinionDetection>(nameof(ReplacementDetection));
        }

        public static IOpinionDetection GetFor(GameObject Perceiver, GameObject Hider, IOpinionDetection Detection, ref AwarenessLevel Level)
        {
            if (FromPool(
                Perceiver: Perceiver,
                Hider: Hider,
                Detection: Detection) is not GetDetectionOpinionEvent E)
                return Detection;

            E.Level = Level;

            E.GetStringyEvent();

            Process(E, out bool _);

            Level = E.Level;
                        
            return E.ReplacementDetection
                ?? E.Detection
                ?? Detection;
        }
    }
}

