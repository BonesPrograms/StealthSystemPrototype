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

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public abstract class Alert<T> : BaseAlert
        where T : IPerception
    {

        #region Constructors

        protected Alert()
            : base()
        {
        }
        protected Alert(T Perception, AlertSource Source)
            : base(Perception, Source)
        {
        }
        public Alert(T Perception, Cell SourceCell)
            : base(Perception, SourceCell)
        {
        }
        public Alert(T Perception, GameObject SourceObject)
            : base(Perception, SourceObject)
        {
        }

        #endregion
    }
}
