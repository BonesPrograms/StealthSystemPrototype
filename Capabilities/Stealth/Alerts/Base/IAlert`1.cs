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

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public class IAlert<T, TSense> : IAlert
        where T : IPerception<TSense>, new()
        where TSense : ISense<TSense>, new()
    {
        #region Constructors

        public IAlert()
            : base()
        {
        }
        protected IAlert(T Perception, TSense Sense, AwarenessLevel Level, bool OverridesCombat, AlertSource Source)
            : base(Perception, Sense, Level, OverridesCombat, Source)
        {
        }
        public IAlert(SenseContext<TSense> Context, ISense Sense, AwarenessLevel Level, bool OverridesCombat)
            : base(Context, Sense, Level, OverridesCombat)
        {
        }
        public IAlert(IAlert<T, TSense> Source)
            : base(Source)
        {
        }

        #endregion

        public override void Create()
        {
            base.Create();
            AfterAlertEvent.Send(SourceObject, ParentObject, this);
        }

        public override IAlert Copy()
            => new IAlert<T, TSense>(this);

        public static IAlert<T, TSense> NewFromContext(SenseContext<TSense> Context, ISense Sense, AwarenessLevel Level, bool? OverridesCombat = null)
            => new IAlert<T, TSense>()
                .FromSenseContext<T, TSense>(Context)
                .SetSense<T, TSense>(Sense)
                .SetAwarenessLevel<T, TSense>(Level)
                .SetOverridesCombat<T, TSense>(OverridesCombat);

        public static TAlert Copy<TAlert>(TAlert SourceAlert)
            where TAlert : IAlert<T, TSense>
            => new IAlert<T, TSense>(SourceAlert) as TAlert;
    }
}
