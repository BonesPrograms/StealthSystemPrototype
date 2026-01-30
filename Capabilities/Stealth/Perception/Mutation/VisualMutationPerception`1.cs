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
    public class VisualMutationPerception<T>
        : BaseMutationPerception<T>
        , IVisualPerception
        where T : BaseMutation
    {
        public override IPurview Purview => GetTypedPurview();

        private LightLevel _MinimumLightLevel = IVisualPerception.DefaultMinimumLightLevel;
        public virtual LightLevel MinimumLightLevel
        {
            get => _MinimumLightLevel;
            protected set => _MinimumLightLevel = value;
        }

        #region Constructors

        public VisualMutationPerception()
            : base()
        {
        }
        public VisualMutationPerception(
            GameObject Owner,
            T Source,
            int Level,
            LightLevel MinimumLightLevel,
            VisualPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
            this.MinimumLightLevel = MinimumLightLevel;
        }
        public VisualMutationPerception(
            GameObject Owner,
            T Source,
            int Level,
            VisualPurview Purview)
            : this(Owner, Source, Level, IVisualPerception.DefaultMinimumLightLevel, Purview)
        {
        }
        public VisualMutationPerception(
            T Source,
            int Level,
            LightLevel MinimumLightLevel,
            VisualPurview Purview)
            : this(null, Source, Level, MinimumLightLevel, Purview)
        {
        }
        public VisualMutationPerception(
            T Source,
            int Level,
            VisualPurview Purview)
            : this(Source, Level, IVisualPerception.DefaultMinimumLightLevel, Purview)
        {
        }
        public VisualMutationPerception(
            GameObject Owner,
            T Source,
            int Level,
            LightLevel MinimumLightLevel,
            int Purview)
            : this(Owner, Source, Level, MinimumLightLevel, new VisualPurview(Purview))
        {
        }
        public VisualMutationPerception(
            GameObject Owner,
            T Source,
            int Level,
            int Purview)
            : this(Owner, Source, Level, IVisualPerception.DefaultMinimumLightLevel, Purview)
        {
        }
        public VisualMutationPerception(
            T Source,
            int Level,
            LightLevel MinimumLightLevel,
            int Purview)
            : this(null, Source, Level, MinimumLightLevel, Purview)
        {
        }
        public VisualMutationPerception(
            T Source,
            int Level,
            int Purview)
            : this(Source, Level, IVisualPerception.DefaultMinimumLightLevel, Purview)
        {
        }

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, IPurview<Visual> Purview)
            => Writer.Write(Purview);

        public sealed override void WritePurview(SerializationWriter Writer, IPurview Purview)
        {
            if (Purview is not VisualPurview typedPurview)
                typedPurview = _Purview as VisualPurview;

            WritePurview(Writer, typedPurview);

            if (typedPurview == null)
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Serialize Write " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(WritePurview) + " override.");
        }

        public virtual void ReadPurview(
            SerializationReader Reader,
            ref IPurview<Visual> Purview,
            IAlertTypedPerception<Visual> ParentPerception = null)
            => Purview = Reader.ReadComposite<VisualPurview>();

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not IPurview<Visual> typedPurview)
                typedPurview = _Purview as IPurview<Visual>;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Visual> ?? this);
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
            Writer.WriteOptimized((byte)_MinimumLightLevel);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            _MinimumLightLevel = (LightLevel)(byte)Reader.ReadOptimizedInt16();
        }

        #endregion

        public override Type GetAlertType()
            => typeof(Visual);

        public virtual IPurview<Visual> GetTypedPurview()
            => (_Purview ??= new VisualPurview(this)) as VisualPurview;

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
    }
}
