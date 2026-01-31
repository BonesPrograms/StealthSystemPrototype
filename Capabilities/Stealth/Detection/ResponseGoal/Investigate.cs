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
using StealthSystemPrototype.Detetection.Opinions;

namespace StealthSystemPrototype.Detetection.ResponseGoals
{
    [Serializable]
    public class Investigate : BaseDetectionResponse
    {
        #region Constructors

        public Investigate()
            : base()
        {
        }
        public Investigate(IOpinionDetection SourceOpinion)
            : base(SourceOpinion)
        {
        }

        #endregion

        public override ResponseGrammar GetResponseGrammar()
            => new()
            {
                Verb = "investigate",
                Verbed = "investigated",
                Verbing = "investigating",
            };

        public override void Create()
        {
            base.Create();
        }
    }
}
