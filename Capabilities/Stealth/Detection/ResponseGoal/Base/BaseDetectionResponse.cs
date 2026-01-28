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
    [StealthSystemBaseClass]
    [Serializable]
    public class BaseDetectionResponse : IDetectionResponseGoal
    {
        #region Constructors

        public BaseDetectionResponse()
            : base()
        {
        }
        public BaseDetectionResponse(IOpinionDetection SourceOpinion)
            : base(SourceOpinion)
        {
        }

        #endregion

        public override ResponseGrammar GetResponseGrammar()
            => new()
            {
                Verb = "detect",
                Verbed = "detected",
                Verbing = "detecting",
            };

        public override void Create()
        {
            base.Create();
        }
    }
}
