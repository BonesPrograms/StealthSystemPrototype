using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;

using StealthSystemPrototype.Events;
using XRL.World.AI;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class Alert<T> : BaseAlert
        where T : BasePerception
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
