using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Genkit;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;

using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Detetections
{
    [Serializable]
    public struct ResponseGrammar : IComposite
    {
        public string Verb;
        public string Verbed;
        public string Verbing;

        public ResponseGrammar(string Verb, string Verbed, string Verbing)
        {
            this.Verb = Verb;
            this.Verbed = Verbed;
            this.Verbing = Verbing;
        }
    }
}
