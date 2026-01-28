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
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Detetection.ResponseGoals
{
    [Serializable]
    public class Search : BaseDetectionResponse
    {
        #region Constructors

        public Search()
            : base()
        {
        }
        public Search(IOpinionDetection SourceOpinion)
            : base(SourceOpinion)
        {
        }

        #endregion

        public override ResponseGrammar GetResponseGrammar()
            => new()
            {
                Verb = "search",
                Verbed = "searched",
                Verbing = "searching",
            };

        public override void Create()
        {
            base.Create();
        }
    }
}
