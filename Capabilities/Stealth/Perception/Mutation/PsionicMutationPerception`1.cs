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
        public override BasePurview Purview => new PsionicPurview(this);

        public virtual Type AlertType => typeof(Psionic);

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
            PsionicAttunement Attunement)
            : base(Owner, Source, Level)
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
            PsionicAttunement Attunement)
            : this(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  RequiresConsciousness: RequiresConsciousness,
                  IgnoreMentalShield: IgnoreMentalShield,
                  Attunement: Attunement)
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
                  Level: Level)
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
            => AlertType;

        public V GetTypedPurview<V>()
            where V : BasePurview<Psionic>
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