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
using StealthSystemPrototype.Detetection.ResponseGoals;

namespace StealthSystemPrototype.Detetection.Opinions
{
    [Serializable]
    public class Curious : IOpinionDetection
    {
        public override IDetectionResponseGoal Response => new Investigate();

        public override int BaseValue => 0;
        #region Constructors

        public Curious()
            : base()
        {
        }

        #endregion

    }
}
