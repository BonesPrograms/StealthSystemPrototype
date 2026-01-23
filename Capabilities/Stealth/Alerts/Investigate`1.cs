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

        public Investigate()
            : base()
        {
            Grammar = new()
            {
                Verb = "investigate",
                Verbed = "investigated",
                Verbing = "investigating",
            };
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
