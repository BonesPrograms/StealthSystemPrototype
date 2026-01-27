using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.AI;

using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Detetections
{
    [Serializable]
    public class Detection<TPerception, TAlert> : IDetectionResponseGoal
        where TPerception : IPerception, new()
        where TAlert : IAlert, new()
    {
        #region Constructors

        public Detection()
            : base()
        {
        }
        protected Detection(TPerception Perception, TAlert Alert, AwarenessLevel Level, bool OverridesCombat, DetectionSource Source)
            : base(Perception as IPerception, Alert, Level, OverridesCombat, Source)
        {
        }
        public Detection(AlertContext<TAlert> Context, TAlert Alert, AwarenessLevel Level, bool OverridesCombat)
            : base(Context, Alert, Level, OverridesCombat)
        {
        }
        public Detection(Detection<TPerception, TAlert> Source)
            : base(Source)
        {
        }

        #endregion

        public override void Create()
        {
            base.Create();
            AfterAlertEvent.Send(SourceObject, ParentObject, this);
        }

        public override IDetectionResponseGoal Copy()
            => new Detection<TPerception, TAlert>(this);

        public static Detection<TPerception, TAlert> NewFromContext(AlertContext<TAlert> Context, ISense Sense, AwarenessLevel Level, bool? OverridesCombat = null)
            => new Detection<TPerception, TAlert>()
                .FromSenseContext<TPerception, TAlert>(Context)
                .SetSense<TPerception, TAlert>(Sense)
                .SetAwarenessLevel<TPerception, TAlert>(Level)
                .SetOverridesCombat<TPerception, TAlert>(OverridesCombat);

        public static TAlert Copy<TAlert>(TAlert SourceAlert)
            where TAlert : Detection<TPerception, TAlert>
            => new Detection<TPerception, TAlert>(SourceAlert) as TAlert;
    }
}
