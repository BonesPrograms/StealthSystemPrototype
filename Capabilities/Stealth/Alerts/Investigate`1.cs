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
    public class Investigate<TSense> : IAlert<IPerception<TSense>, TSense>
        where TSense : ISense<TSense>, new()
    {
        #region Constructors

        protected Investigate()
            : base()
        {
        }
        protected Investigate(IPerception<TSense> Perception, TSense Sense, AwarenessLevel Level, AlertSource Source)
            : base(Perception, Sense, Level, Source)
        {
        }
        public Investigate(IPerception<TSense> Perception, TSense Sense, AwarenessLevel Level, Cell SourceCell)
            : base(Perception, Sense, Level, SourceCell)
        {
        }
        public Investigate(IPerception<TSense> Perception, TSense Sense, AwarenessLevel Level, GameObject SourceObject)
            : base(Perception, Sense, Level, SourceObject)
        {
        }
        public Investigate(SenseContext<TSense> Context, ISense Sense, AwarenessLevel Level)
            : base(Context, Sense, Level)
        {
        }
        public Investigate(Investigate<TSense> Source)
            : base(Source)
        {
        }

        #endregion

        public override void Create()
        {
            base.Create();
            Think("I will " + GetType().ToStringWithGenerics() + " the " + nameof(Cell) + " at " + 
                (SourceCell?.Location?.ToString() ?? "NO_LOCATION") + 
                " because something there made me " + Level.ToString());
        }
    }
}
