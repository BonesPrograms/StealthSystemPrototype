using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Senses;

using XRL.Liquids;
using XRL.World;
using XRL.World.Effects;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="Psionic"/> <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    public interface IPsionicPerception : IAlertTypedPerception<Psionic>
    {
        public bool RequiresConsciousness => true;

        public bool CanOnlyDetectTarget { get; }

        public bool CanOnlyDetectLiving { get; }

        public bool CanOnlyDetectBrain { get; }

        public bool CanOnlyDetectEligibleForMentalMutations { get; }

        public bool CanOnlyDetectWithMentalMutations { get; }

        public new bool CanPerceive(AlertContext Context)
        {
            if (!((IPerception)this).CanPerceive(Context))
                return false;

            if (RequiresConsciousness
                && Owner.HasEffect<Asleep>())
                return false;

            GameObject actor = Context?.Actor;
            if (CanOnlyDetectTarget
                && actor == null)
                return false;

            if (CanOnlyDetectLiving
                && !actor.IsAlive)
                return false;

            if (CanOnlyDetectBrain
                && actor.Brain == null)
                return false;

            if (CanOnlyDetectEligibleForMentalMutations
                && !actor.EligibleForMentalMutations())
                return false;

            return !CanOnlyDetectWithMentalMutations
                || actor.HasMentalMutations();
        }
    }
}
