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
    /// Represents the record of a <see cref="BasePerception"/> having successfully detected a <see cref="BaseAlert"/> within an <see cref="IConcealedAction"/>, and handles the pushing of its <see cref="IDetectionResponseGoal"/>.
    /// </summary>
    [StealthSystemBaseClass]
    public abstract class IOpinionDetection : IOpinion
    {
        public abstract IDetectionResponseGoal Response { get; }

        public GameObject Perceiver;

        public GameObject Sneaker;

        public GameObject AlertObject;

        public Cell AlertLocation;

        public AwarenessLevel Level;

        public abstract int BaseDuration { get; }

        protected int _Duration;
        public override int Duration => _Duration;

        public long RemainingTime
            => Duration > 0
            ? Duration - (The.Game.TimeTicks - Time)
            : 0L;

        public IOpinionDetection()
            : base()
        {
            Perceiver = null;
            Sneaker = null;
            AlertObject = null;
            Level = AwarenessLevel.None;
            _Duration = BaseDuration;
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteGameObject(Perceiver);
            Writer.WriteGameObject(Sneaker);
            Writer.WriteGameObject(AlertObject);
            Writer.Write(AlertLocation);
            Writer.WriteOptimized((int)Level);
            Writer.WriteOptimized(_Duration);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Perceiver = Reader.ReadGameObject();
            Sneaker = Reader.ReadGameObject();
            AlertObject = Reader.ReadGameObject();
            AlertLocation = Reader.ReadCell();
            Level = (AwarenessLevel)Reader.ReadOptimizedInt32();
            _Duration = Reader.ReadOptimizedInt32();
        }

        #endregion

        public virtual void Initialize(AlertContext AlertContext, AwarenessLevel Level)
        {
            Perceiver = AlertContext.Perceiver;
            Sneaker = AlertContext.Sneaker;
            AlertObject = AlertContext.AlertObject;
            AlertLocation = AlertContext.AlertLocation;
            Response.Initialize(this);

            AfterDetectedEvent.Send(AlertContext.Perceiver, AlertContext.Sneaker, this);
            AlertContext.Perceiver.Brain.PushGoal(Response);
        }

        public virtual IOpinionDetection DeepCopy(GameObject Perceiver)
        {
            var opinionDetection = Activator.CreateInstance(GetType()) as IOpinionDetection;

            var fields = GetType().GetFields();

            foreach (var fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(opinionDetection, fieldInfo.GetValue(this));

            opinionDetection.Initialize(AlertContext.DeepCopy(Perceiver), Level);

            return opinionDetection;
        }
    }
}
