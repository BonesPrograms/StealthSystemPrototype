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
    public class Search<TSense> : Detection<IPerception<TSense>, TSense>
        where TSense : ISense<TSense>, new()
    {
        #region Constructors

        protected Search()
            : base()
        {
        }

        #endregion

        public override void Create()
        {
            base.Create();
            Think("I will " + GetType().ToStringWithGenerics() + " the " + nameof(Cell).Pluralize() + " adjacent to " + 
                (SourceCell?.Location?.ToString() ?? "NO_LOCATION") + 
                " because something there made me " + Level.ToString() + " and nothing was there.");
        }
    }
}
