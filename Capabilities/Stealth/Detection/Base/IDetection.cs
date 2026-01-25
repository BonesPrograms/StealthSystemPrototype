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

namespace StealthSystemPrototype.Detetections
{
    /// <summary>
    /// Contracts a class (typically a <see cref="GoalHandler"/>) as being capable of handling goals related to the detection of <see cref="IAlert"/>s in <see cref="IConcealedAction"/>s, by <see cref="IPerception"/>s.
    /// </summary>
    public interface IDetection : IComposite
    {
        #region GoalHandler Properties & Fields

        public GameObject ParentObject { get; }

        public Brain ParentBrain { get; }

        public Zone CurrentZone { get; }

        public Cell CurrentCell { get; }

        #endregion

        public Guid ID { get; }

        public string Name { get; }

        public DetectionSource Source { get; }

        public DetectionGrammar Grammar { get; }

        public IPerception Perception { get; }

        public IAlert Alert { get; }

        public Cell Origin { get; }

        public AwarenessLevel Level { get; }

        public int Priority { get; }

        public bool OverridesCombat { get; }

        public bool ValidateDetection()
            => ValidateDetection(this);

        public static bool ValidateDetection(IDetection Detection)
            => Detection != null
            && Detection.ParentObject is GameObject parentObject
            && GameObject.Validate(ref parentObject)
            && Detection.Perception != null
            && Detection.Origin != null;

        public static bool ValidateDetection(ref IDetection Alert)
        {
            if (!ValidateDetection(Alert))
            {
                Alert = null;
                return false;
            }
            return true;
        }

        public IDetection Copy();

        protected IDetection FromAlertContext(AlertContext Context);

        protected IDetection SetAlert(IAlert Alert);

        protected IDetection SetAwarenessLevel(AwarenessLevel Level);

        protected IDetection SetSource(DetectionSource Source);

        protected IDetection SetOverridesCombat(bool? OverridesCombat);

        public void WriteDetection(SerializationWriter Writer, IDetection Detection);

        public void ReadDetection(SerializationReader Reader, IDetection Detection);

        public new void Write(SerializationWriter Writer)
        {
            WriteDetection(Writer, this);
        }

        public new void Read(SerializationReader Reader)
        {
            ReadDetection(Reader, this);
        }
    }
}
