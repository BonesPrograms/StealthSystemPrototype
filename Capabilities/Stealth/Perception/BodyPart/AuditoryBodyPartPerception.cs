using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class AuditoryBodyPartPerception
        : BaseBodyPartPerception
        , IAuditoryPerception
    {
        public virtual AuditoryPurview Purview
        {
            get => _Purview as AuditoryPurview;
            set => _Purview = value;
        }

        #region Constructors

        public AuditoryBodyPartPerception()
            : base()
        {
        }
        public AuditoryBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            AuditoryPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
        }
        public AuditoryBodyPartPerception(
            BodyPart Source,
            int Level,
            AuditoryPurview Purview)
            : this(null, Source, Level, Purview)
        {
        }
        public AuditoryBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            int Purview)
            : this(Owner, Source, Level, new AuditoryPurview(Purview))
        {
        }
        public AuditoryBodyPartPerception(
            BodyPart Source,
            int Level,
            int Purview)
            : this(null, Source, Level, Purview)
        {
        }
        public AuditoryBodyPartPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
        {
        }

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, AuditoryPurview Purview)
            => Writer.Write(Purview);

        public sealed override void WritePurview(SerializationWriter Writer, IPurview Purview)
        {
            if (Purview is not AuditoryPurview typedPurview)
                typedPurview = _Purview as AuditoryPurview;

            WritePurview(Writer, typedPurview);

            if (typedPurview == null)
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Serialize Write " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(WritePurview) + " override.");
        }

        public virtual void ReadPurview(
            SerializationReader Reader,
            ref AuditoryPurview Purview,
            IAlertTypedPerception<Auditory, AuditoryPurview> ParentPerception = null)
            => Purview = new AuditoryPurview(Reader, ParentPerception ?? this);

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not AuditoryPurview typedPurview)
                typedPurview = _Purview as AuditoryPurview;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Auditory, AuditoryPurview> ?? this);
                _Purview = typedPurview;
            }
            else
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Read Serialzed " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(ReadPurview) + " override.");
        }

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        #endregion

        public override void Construct()
        {
            base.Construct();
            // put default instance values here for the base parameterless constructor
        }

        public override Type GetAlertType()
            => ((IAlertTypedPerception<Auditory, AuditoryPurview>)this).GetAlertType();

        public override void AssignDefaultPurview(int Value)
            => Purview = GetDefaultPurview(Value);

        public override IPurview GetDefaultPurview(int Value)
            => GetDefaultPurview(
                Value: Value,
                purviewArgs: new object[]
                {
                    AuditoryPurview.DefaultDiffuser.SetSteps(Value),
                });

        public virtual AuditoryPurview GetDefaultPurview(int Value, params object[] purviewArgs)
        {
            var purview = new AuditoryPurview(this as IAlertTypedPerception<Auditory, IPurview<Auditory>>, Value);
            if (!purviewArgs.IsNullOrEmpty())
                foreach (object arg in purviewArgs)
                {
                    if (arg is BaseDoubleDiffuser diffuserArg)
                        purview.Diffuser = diffuserArg;
                }
            return purview;
        }

        public override bool CanPerceiveAlert(IAlert Alert)
            => ((IAlertTypedPerception<Visual, IPurview<Visual>>)this).CanPerceiveAlert(Alert);

        public override bool TryPerceive(AlertContext Context)
            => base.TryPerceive(Context);

        public override IOpinionDetection RaiseDetection(AlertContext Context)
            => base.RaiseDetection(Context);

        public override BodyPart GetSource()
            => ((IBodyPartPerception)this).GetSource();

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}