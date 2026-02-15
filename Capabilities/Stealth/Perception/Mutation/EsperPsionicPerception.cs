using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Parts;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Perceptions.IPsionicPerception;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class EsperPsionicPerception : PsionicMutationPerception<Esper>
    {
        public override Esper Source
        {
            get => base.Source = _Perceiver?.GetPart<Esper>();
            set => base.Source = _Perceiver?.GetPart<Esper>();
        }
        public override BasePurview Purview
        {
            get => _Purview ??= new EsperPurview(this);
            protected set => base.Purview = value;
        }

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

                    short derivedAttunement = (short)(combinedLevels.Clamp(1, count) / count.Clamp(1, count) / (Perceiver?.Level ?? 1))
                        .Clamp((short)PsionicAttunement.Espers, (short)PsionicAttunement.Total);

                    return (PsionicAttunement)derivedAttunement;
                }
                else
                    return PsionicAttunement.MentalMutations;
            }
        }

        public Mutations OwnerMutationsPart => Perceiver?.GetPart<Mutations>();

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

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
        }

        #endregion

        public override int GetLevelAdjustment(int Level = 0)
            => base.GetLevelAdjustment(Level) + (Perceiver?.StatMod("Ego") ?? 0);
    }
}