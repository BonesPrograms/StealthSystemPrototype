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
using XRL.World.Parts;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class EsperPsionicPerception : PsionicMutationPerception<Esper>
    {
        public override bool RequiresConsciousness => true;
        public override bool IgnoreMentalShield => false;
        public override PsionicAttunement Attunement
        {
            get
            {
                if (!OrderedOwnerMentalMutations.IsNullOrEmpty())
                {
                    int count = OrderedOwnerMentalMutations.Count.Clamp(0, 5);
                    List<BaseMutation> shortOrderedMentalMutations = new();
                    for (int i = 0; i < count; i++)
                        shortOrderedMentalMutations[i] = OrderedOwnerMentalMutations[i];
                    int combinedLevels = shortOrderedMentalMutations.Aggregate(
                        seed: 0,
                        func: (a, n) => a + n.Level);

                    short derivedAttunement = (short)(combinedLevels.Clamp(1, count) / count.Clamp(1, count) / (Owner?.Level ?? 1))
                        .Clamp((short)PsionicAttunement.Espers, (short)PsionicAttunement.Total);

                    return (PsionicAttunement)derivedAttunement;
                }
                else
                    return PsionicAttunement.MentalMutations;
            }
        }

        public Mutations OwnerMutationsPart => Owner?.GetPart<Mutations>();

        public List<BaseMutation> OrderedOwnerMentalMutations => OwnerMutationsPart
            ?.ActiveMutationList
            ?.Where(bm => bm.IsMental() && bm.GetMutationClass() != nameof(Esper))
            ?.OrderByDescending(bm => bm.Level)
            ?.ToList();

        #region Constructors

        public EsperPsionicPerception()
            : base()
        {
        }
        public EsperPsionicPerception(
            GameObject Owner,
            Esper Source,
            int Level,
            PsionicPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
        }
        public EsperPsionicPerception(
            Esper Source,
            int Level,
            PsionicPurview Purview)
            : this(
                  Owner: null,
                  Source: Source,
                  Level: Level,
                  Purview: Purview)
        {
        }
        public EsperPsionicPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
        {
        }

        #endregion
        #region Serialization

        public override void WritePurview(SerializationWriter Writer, PsionicPurview Purview)
            => Writer.Write(Purview);

        public override void ReadPurview(
            SerializationReader Reader,
            ref PsionicPurview Purview,
            IAlertTypedPerception<Psionic, PsionicPurview> ParentPerception = null)
            => Purview = new PsionicPurview(Reader, ParentPerception ?? this);

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
        }

        #endregion

        public override void Construct()
            => base.Construct();

        public override int GetLevelAdjustment(int Level = 0)
            => base.GetLevelAdjustment(Level) + (Owner?.StatMod("Ego") ?? 0);

        public override bool CanPerceiveAlert(IAlert Alert)
            => ((IAlertTypedPerception<Visual, IPurview<Visual>>)this).CanPerceiveAlert(Alert);

        public override bool TryPerceive(AlertContext Context)
            => base.TryPerceive(Context);

        public override IDetection RaiseDetection(AlertContext Context)
            => base.RaiseDetection(Context);

        public override Esper GetSource()
            => base.GetSource();
    }
}