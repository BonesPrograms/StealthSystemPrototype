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
    public abstract class Alert<T, TSense> : IAlert
        where T : IPerception<TSense>, new()
        where TSense : ISense<TSense>, new()
    {
        #region Constructors

        protected Alert()
            : base()
        {
        }
        protected Alert(T Perception, TSense Sense, AlertSource Source)
            : base(Perception, Sense, Source)
        {
        }
        public Alert(T Perception, TSense Sense, Cell SourceCell)
            : base(Perception, Sense, SourceCell)
        {
        }
        public Alert(T Perception, TSense Sense, GameObject SourceObject)
            : base(Perception, Sense, SourceObject)
        {
        }

        #endregion
    }
}
