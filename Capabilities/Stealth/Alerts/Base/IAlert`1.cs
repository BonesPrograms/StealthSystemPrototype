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

        protected IAlert()
            : base()
        {
        }
        protected IAlert(T Perception, TSense Sense, AwarenessLevel Level, AlertSource Source)
            : base(Perception, Sense, Level, Source)
        {
        }
        public IAlert(T Perception, TSense Sense, AwarenessLevel Level, Cell SourceCell)
            : base(Perception, Sense, Level, SourceCell)
        {
        }
        public IAlert(T Perception, TSense Sense, AwarenessLevel Level, GameObject SourceObject)
            : base(Perception, Sense, Level, SourceObject)
        {
        }
        public IAlert(SenseContext<TSense> Context, ISense Sense, AwarenessLevel Level)
            : base(Context, Sense, Level)
        {
        }
        public IAlert(IAlert<T, TSense> Source)
            : base(Source)
        {
        }

        #endregion

        public override IAlert Copy()
            => new IAlert<T, TSense>(this);

        public static TAlert NewFromContext<TAlert>(SenseContext<TSense> Context, ISense Sense, AwarenessLevel Level)
            where TAlert : IAlert<T, TSense>
            => new IAlert<T, TSense>(Context, Sense, Level) as TAlert;

        public static TAlert Copy<TAlert>(TAlert SourceAlert)
            where TAlert : IAlert<T, TSense>
            => new IAlert<T, TSense>(SourceAlert) as TAlert;
    }
}
