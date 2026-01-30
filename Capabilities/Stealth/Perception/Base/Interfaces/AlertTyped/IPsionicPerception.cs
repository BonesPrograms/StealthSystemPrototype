using System;
using System.Collections.Generic;

using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="Psionic"/> <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/> by way of an <see cref="PsionicPurview"/>.
    /// </summary>
    public interface IPsionicPerception : IAlertTypedPerception<Psionic>
    {
        /// <summary>
        /// Represents the minimum requirements of the <see cref="IConcealedAction.Actor"/> for the <see cref="IPsionicPerception.Owner"/> to be able to detect them.
        /// </summary>
        [Serializable]
        public enum PsionicAttunement : short
        {
            /// <summary>Non-value. Serves as "default", and indicates inability to detect.</summary>
            None,
            /// <summary>The <see cref="IConcealedAction.Actor"/> is an <see cref="Esper"/>.</summary>
            Espers,
            /// <summary>The <see cref="IConcealedAction.Actor"/> is capable of acquiring mental mutations, whether they have any or not.</summary>
            NonChimera,
            /// <summary>The <see cref="IConcealedAction.Actor"/> has at least one mental mutation, including temporarily.</summary>
            MentalMutations,
            /// <summary>The <see cref="IConcealedAction.Actor"/> has a <see cref="Brain"/>.</summary>
            Sentient,
            /// <summary>The <see cref="IConcealedAction.Actor"/> <see cref="GameObject.IsAlive"/>.</summary>
            Living,
            /// <summary>The <see cref="IConcealedAction.Actor"/> needn't actually presently exist. Lingering <see cref="Psionic"/> "energy" is detectable.</summary>
            Ambient,
            /// <summary>Any an all <see cref="Psionic"/> activity is detectable, whatever that might look like.</summary>
            Total,
        }

        public static PsionicAttunement DefaultAttunement => PsionicAttunement.Sentient;

        public bool RequiresConsciousness => true;

        public bool IgnoreMentalShield => false;

        public PsionicAttunement Attunement => DefaultAttunement;

        public new bool CanPerceive(AlertContext Context)
        {
            if (Attunement == PsionicAttunement.None)
                return false;

            if (!((IPerception)this).CanPerceive(Context))
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
    }
}
