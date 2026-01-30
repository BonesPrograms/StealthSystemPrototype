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
using XRL.World.Effects;
using XRL.World.Parts;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class PsionicMutationPerception<T>
        : BaseMutationPerception<T>
        , IPsionicPerception
        where T : BaseMutation
    {
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

        public PsionicMutationPerception()
            : base()
        {
        }
        public PsionicMutationPerception(
            GameObject Owner,
            T Source,
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
        public PsionicMutationPerception(
            T Source,
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
        public PsionicMutationPerception(
            GameObject Owner,
            T Source,
            int Level,
            PsionicPurview Purview)
            : base(
                  Owner: Owner,
                  Source: Source,
                  Level: Level,
                  Purview: Purview)
        {
        }
        public PsionicMutationPerception(
            T Source,
            int Level,
            PsionicPurview Purview)
            : this(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  Purview: Purview)
        {
        }

        #endregion
        #region Serialization

        public virtual void WritePurview(SerializationWriter Writer, IPurview<Psionic> Purview)
            => Writer.Write(Purview);

        public sealed override void WritePurview(SerializationWriter Writer, IPurview Purview)
        {
            if (Purview is not IPurview<Psionic> typedPurview)
                typedPurview = _Purview as IPurview<Psionic>;

            WritePurview(Writer, typedPurview);

            if (typedPurview == null)
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: GetType().ToStringWithGenerics() + " Failed to Serialize Write " +
                        Purview.TypeStringWithGenerics() + " from untyped " + nameof(WritePurview) + " override.");
        }

        public virtual void ReadPurview(
            SerializationReader Reader,
            ref IPurview<Psionic> Purview,
            IAlertTypedPerception<Psionic> ParentPerception = null)
            => Purview = Reader.ReadComposite<PsionicPurview>();

        public sealed override void ReadPurview(
            SerializationReader Reader,
            ref IPurview Purview,
            IPerception ParentPerception = null)
        {
            if (Purview is not IPurview<Psionic> typedPurview)
                typedPurview = _Purview as IPurview<Psionic>;

            if (typedPurview != null)
            {
                ReadPurview(Reader, ref typedPurview, ParentPerception as IAlertTypedPerception<Psionic> ?? this);
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
            => typeof(Psionic);

        public virtual IPurview<Psionic> GetTypedPurview()
            => (_Purview ??= new PsionicPurview(this)) as PsionicPurview;

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

        public override bool CanPerceive(AlertContext Context)
        {
            if (Attunement == PsionicAttunement.None)
                return false;

            if (!base.CanPerceive(Context))
                return false;

            if (RequiresConsciousness
                && Owner.HasEffect<Asleep>())
                return false;

            GameObject actor = Context?.Hider;
            if (actor == null)
                return Attunement >= PsionicAttunement.Ambient;

            if (!IgnoreMentalShield
                && actor.HasPart<MentalShield>())
                return false;

            if (Attunement >= PsionicAttunement.Living
                && actor.IsAlive)
                return true;

            if (Attunement >= PsionicAttunement.Sentient
                && actor.Brain != null)
                return true;

            if (Attunement >= PsionicAttunement.MentalMutations
                && actor.HasMentalMutations())
                return true;

            if (Attunement >= PsionicAttunement.NonChimera
                && actor.EligibleForMentalMutations())
                return true;

            if (Attunement >= PsionicAttunement.Espers
                && actor.HasPart<Esper>())
                return true;

            return false;
        }

        public override bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin)
            => base.TryPerceive(Context, out SuccessMargin, out FailureMargin);

        public override IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin)
            => base.RaiseDetection(Context, SuccessMargin);

        public override T GetSource()
            => base.GetSource();
    }
}