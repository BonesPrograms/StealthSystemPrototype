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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Detetections
{
    /// <summary>
    /// Represents the record of an <see cref="IPerception"/> having successfully detected an <see cref="IAlert"/> within an <see cref="IConcealedAction"/>, and handles the pushing of .
    /// </summary>
    [StealthSystemBaseClass]
    public abstract class IOpinionDetection
        : IOpinion
    {
        public abstract IDetectionResponseGoal Response { get; }

        public GameObject Subject;

        public GameObject Object;

        public IAlertTypedPerception Perception;

        public IAlert Alert;

        public Cell Origin;

        public AwarenessLevel Level;

        public IOpinionDetection()
            : base()
        {
            Subject = null;
            Object = null;
            Perception = null;
            Alert = null;
            Origin = null;
            Level = AwarenessLevel.None;
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {

        }

        public override void Read(SerializationReader Reader)
        {

        }

        #endregion

        public virtual void Initialize(AlertContext Context)
        {
            Response.Initialize(Context);
            Actor.Brain.PushGoal(Response);
        }

        public abstract IOpinionDetection Copy();

        protected abstract IOpinionDetection FromAlertContext(AlertContext Context);

        protected abstract IOpinionDetection SetAlert(IAlert Alert);

        protected abstract IOpinionDetection SetAwarenessLevel(AwarenessLevel Level);
    }
}
