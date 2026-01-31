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
        public override BasePurview Purview => new AuditoryPurview(this);

        public virtual Type AlertType => typeof(Auditory);

        #region Constructors

        public AuditoryBodyPartPerception()
            : base()
        {
        }
        public AuditoryBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            int? PurviewValue = null)
            : base(Owner, Source, Level, PurviewValue)
        {
        }
        public AuditoryBodyPartPerception(
            BodyPart Source,
            int Level,
            int? PurviewValue = null)
            : this(null, Source, Level, PurviewValue)
        {
        }

        #endregion
        #region Serialization

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
            => AlertType;

        public V GetTypedPurview<V>()
            where V : BasePurview<Auditory>
            => Purview as V;

        public override BasePurview GetPurview()
            => Purview;

        public override void ConfigurePurview(int Value, Dictionary<string, object> args = null)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                });

            args ??= new();
            args[nameof(Value)] = Value;
            args[nameof(IDiffusingPurview.ConfigureDiffuser)] = new Dictionary<string, object>()
            {
                { nameof(BasePurview.ParentPerception), this },
                { nameof(IDiffusingPurview.Diffuser.SetSteps), Value },
            };

            args.ForEach(kvp => Debug.Log(kvp.Key, kvp.Value, Indent: indent[1]));

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