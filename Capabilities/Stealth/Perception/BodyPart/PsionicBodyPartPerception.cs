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
using static StealthSystemPrototype.Perceptions.IPsionicPerception;
using StealthSystemPrototype.Detetection.Opinions;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class PsionicBodyPartPerception
        : BaseBodyPartPerception
        , IPsionicPerception
    {
        public PsionicPurview Purview
        {
            get => _Purview as PsionicPurview;
            set => _Purview = value;
        }

        private bool _RequiresConsciousness = true;
        public virtual bool RequiresConsciousness
        {
            get => _RequiresConsciousness;
            protected set => _RequiresConsciousness = value;
        }

        private bool _IgnoreMentalShield = false;
        public virtual bool IgnoreMentalShield
        {
            get => _IgnoreMentalShield;
            protected set => _IgnoreMentalShield = value;
        }

        private PsionicAttunement _Attunement = DefaultAttunement;
        public virtual PsionicAttunement Attunement
        {
            get => _Attunement;
            protected set => _Attunement = value;
        }

        #region Constructors

        public PsionicBodyPartPerception()
            : base()
        {
        }
        public PsionicBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            bool RequiresConsciousness,
            bool IgnoreMentalShield,
            PsionicAttunement Attunement,
            PsionicPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
            this.RequiresConsciousness = RequiresConsciousness;
            this.IgnoreMentalShield = IgnoreMentalShield;
            this.Attunement = Attunement;
        }
        public PsionicBodyPartPerception(
            BodyPart Source,
            int Level,
            bool RequiresConsciousness,
            bool IgnoreMentalShield,
            PsionicAttunement Attunement,
            PsionicPurview Purview)
            : this(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  RequiresConsciousness: RequiresConsciousness,
                  IgnoreMentalShield: IgnoreMentalShield,
                  Attunement: Attunement,
                  Purview: Purview)
        {
        }
        public PsionicBodyPartPerception(
            BodyPart Source,
            int Level,
            PsionicPurview Purview)
            : base(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  Purview: Purview)
        {
        }

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, PsionicPurview Purview)
            => Writer.Write(Purview);

        public sealed override void WritePurview(SerializationWriter Writer, IPurview Purview)
        {
            if (Purview is not PsionicPurview typedPurview)
                typedPurview = _Purview as PsionicPurview;

            WritePurview(Writer, typedPurview);

            if (typedPurview == null)
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Serialize Write " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(WritePurview) + " override.");
        }

        public virtual void ReadPurview(
            SerializationReader Reader,
            ref PsionicPurview Purview,
            IAlertTypedPerception<Psionic, PsionicPurview> ParentPerception = null)
            => Purview = Reader.ReadComposite<PsionicPurview>();

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not PsionicPurview typedPurview)
                typedPurview = _Purview as PsionicPurview;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Psionic, PsionicPurview> ?? this);
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
            Writer.Write(RequiresConsciousness);
            Writer.Write(IgnoreMentalShield);
            Writer.WriteOptimized((short)Attunement);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            RequiresConsciousness = Reader.ReadBoolean();
            IgnoreMentalShield = Reader.ReadBoolean();
            Attunement = (PsionicAttunement)Reader.ReadOptimizedInt16();
        }

        #endregion

        public override Type GetAlertType()
            => ((IAlertTypedPerception<Psionic, PsionicPurview>)this).GetAlertType();

        public override void AssignDefaultPurview(int Value)
            => Purview = GetDefaultPurview(Value);

        public override IPurview GetDefaultPurview(int Value)
            => GetDefaultPurview(
                Value: Value,
                purviewArgs: new object[]
                {
                    PsionicPurview.DefaultDiffuser.SetSteps(Value),
                });

        public virtual PsionicPurview GetDefaultPurview(int Value, params object[] purviewArgs)
        {
            var purview = new PsionicPurview(this as IAlertTypedPerception<Psionic, IPurview<Psionic>>, Value);
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