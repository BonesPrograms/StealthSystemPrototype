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
    public class VisualMutationPerception<T>
        : BaseMutationPerception<T>
        , IVisualPerception
        where T : BaseMutation
    {
        public VisualPurview Purview
        {
            get => _Purview as VisualPurview;
            set => _Purview = value;
        }

        public virtual LightLevel MinimumLightLevel { get; protected set; }

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
            MinimumLightLevel = MinimumLightLevel;
        }
        public VisualMutationPerception(
            T Source,
            int Level,
            LightLevel MinimumLightLevel,
            VisualPurview Purview)
            : this(null, Source, Level, MinimumLightLevel, Purview)
        {
        }
        public VisualMutationPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
        {
        }

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, VisualPurview Purview)
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
            ref VisualPurview Purview,
            IAlertTypedPerception<Visual, VisualPurview> ParentPerception = null)
            => Purview = new VisualPurview(Reader, ParentPerception ?? this);

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not VisualPurview typedPurview)
                typedPurview = _Purview as VisualPurview;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Visual, VisualPurview> ?? this);
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
            Writer.WriteOptimized((byte)MinimumLightLevel);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            MinimumLightLevel = (LightLevel)(byte)Reader.ReadOptimizedInt16();
        }

        #endregion

        public override void Construct()
        {
            base.Construct();
            MinimumLightLevel = ((IVisualPerception)this).MinimumLightLevel;
        }

        public override bool CanPerceiveAlert(IAlert Alert)
            => ((IAlertTypedPerception<Visual, IPurview<Visual>>)this).CanPerceiveAlert(Alert);

        public override bool TryPerceive(AlertContext Context)
            => base.TryPerceive(Context);

        public override IDetection RaiseDetection(AlertContext Context)
            => base.RaiseDetection(Context);

        public override T GetSource()
            => base.GetSource();
    }
}
