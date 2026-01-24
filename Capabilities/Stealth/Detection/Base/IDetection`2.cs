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
    /// <summary>
    /// Contracts a class (typically a <see cref="GoalHandler"/>) as being capable of handling goals related to the detection of an <see cref="{A}"/> typed <see cref="IAlert"/> in <see cref="IConcealedAction"/>s, by a <see cref="{P}"/> typed <see cref="IAlertTypedPerception{A}"/>.
    /// </summary>
    public interface IDetection<P, A> : IDetection
        where P : class, IAlertTypedPerception<A>, new()
        where A : class, IAlert, new()
    {
        public new P Perception { get; }

        public new A Alert { get; }

        public new IDetection<P, A> Copy();

        protected new IDetection<P, A> FromAlertContext(AlertContext Context);

        protected IDetection<P, A> SetAlert(A Alert);

        protected new IDetection<P, A> SetAwarenessLevel(AwarenessLevel Level);

        protected new IDetection<P, A> SetSource(DetectionSource Source);

        protected new IDetection<P, A> SetOverridesCombat(bool? OverridesCombat);
    }
}
