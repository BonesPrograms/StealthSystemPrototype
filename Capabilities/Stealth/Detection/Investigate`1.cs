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

namespace StealthSystemPrototype.Detetections
{
    [Serializable]
    public class Investigate : IDetectionResponseGoal
    {
        private ResponseGrammar? _ResponseGrammar;
        public override ResponseGrammar Grammar => _ResponseGrammar ??= new()
        {
            Verb = "investigate",
            Verbed = "investigated",
            Verbing = "investigating",
        };

        #region Constructors

        public Investigate()
            : base()
        {
            _ResponseGrammar = null;
        }

        #endregion

        public override void Create()
        {
            base.Create();
        }
    }
}
