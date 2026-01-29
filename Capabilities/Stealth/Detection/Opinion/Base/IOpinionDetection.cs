using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using Genkit;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;

using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.ResponseGoals;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Detetection.Opinions
{
    /// <summary>
    /// Represents the record of an <see cref="IPerception"/> having successfully detected an <see cref="IAlert"/> within an <see cref="IConcealedAction"/>, and handles the pushing of .
    /// </summary>
    [StealthSystemBaseClass]
    public abstract class IOpinionDetection : IOpinion
    {
        public abstract IDetectionResponseGoal Response { get; }

        public AlertContext AlertContext;

        public AwarenessLevel Level;

        public IOpinionDetection()
            : base()
        {
            AlertContext = null;
            Level = AwarenessLevel.None;
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            Writer.Write(AlertContext);
            Writer.WriteOptimized((int)Level);
        }

        public override void Read(SerializationReader Reader)
        {
            AlertContext = new(Reader);
            Level = (AwarenessLevel)Reader.ReadOptimizedInt32();
        }

        #endregion

        public virtual void Initialize(AlertContext AlertContext, AwarenessLevel Level)
        {
            this.AlertContext = AlertContext;
            Response.Initialize(this);

            AfterDetectedEvent.Send(AlertContext.Perceiver, AlertContext.Hider, this);
            AlertContext.Perceiver.Brain.PushGoal(Response);
        }

        public virtual IOpinionDetection DeepCopy(GameObject Perceiver)
        {
            IOpinionDetection opinionDetection = Activator.CreateInstance(GetType()) as IOpinionDetection;

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(opinionDetection, fieldInfo.GetValue(this));

            opinionDetection.Initialize(AlertContext.DeepCopy(Perceiver), Level);

            return opinionDetection;
        }
    }
}
