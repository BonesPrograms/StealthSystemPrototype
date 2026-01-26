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
using static StealthSystemPrototype.Perceptions.IPsionicPerception;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class PsionicMutationPerception<T>
        : BaseMutationPerception<T>
        , IPsionicPerception
        where T : BaseMutation
    {
        public PsionicPurview Purview
        {
            get => _Purview as PsionicPurview;
            set => _Purview = value;
        }
        public virtual bool RequiresConsciousness { get; protected set; }
        public virtual bool IgnoreMentalShield { get; protected set; }
        public virtual PsionicAttunement Attunement { get; protected set; }

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
        public PsionicMutationPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
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
            => Purview = new PsionicPurview(Reader, ParentPerception ?? this);

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

        public override void Construct()
        {
            base.Construct();
            RequiresConsciousness = ((IPsionicPerception)this).RequiresConsciousness;
            IgnoreMentalShield = ((IPsionicPerception)this).IgnoreMentalShield;
            Attunement = ((IPsionicPerception)this).Attunement;
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