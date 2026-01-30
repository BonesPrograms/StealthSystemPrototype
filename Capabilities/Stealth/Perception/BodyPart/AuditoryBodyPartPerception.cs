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
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Detetection.Opinions;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class AuditoryBodyPartPerception
        : BaseBodyPartPerception
        , IAuditoryPerception
    {
        public override IPurview Purview => GetTypedPurview();

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

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, IPurview<Auditory> Purview)
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
            ref IPurview<Auditory> Purview,
            IAlertTypedPerception<Auditory> ParentPerception = null)
            => Purview = Reader.ReadComposite<AuditoryPurview>();

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not IPurview<Auditory> typedPurview)
                typedPurview = _Purview as IPurview<Auditory>;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Auditory> ?? this);
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

        public override Type GetAlertType()
            => typeof(Auditory);

        public virtual IPurview<Auditory> GetTypedPurview()
            => (_Purview ??= new AuditoryPurview(this)) as AuditoryPurview;

        public override IPurview GetPurview()
            => Purview;

        public override void ConfigurePurview(int Value, Dictionary<string, object> args = null)
        {
            args ??= new();
            args[nameof(IPurview.Value)] = Value;
            args[nameof(IDiffusingPurview.ConfigureDiffuser)] = new Dictionary<string, object>()
            {
                { nameof(IDiffusingPurview.Diffuser.SetSteps), Value },
            };
            base.ConfigurePurview(Value, args);
        }

        public override bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin)
            => base.TryPerceive(Context, out SuccessMargin, out FailureMargin);

        public override IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin)
            => base.RaiseDetection(Context, SuccessMargin);

        public override BodyPart GetSource()
            => ((IBodyPartPerception)this).GetSource();

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}