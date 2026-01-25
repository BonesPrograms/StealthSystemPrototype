using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using XRL.Liquids;
using XRL.World;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="Auditory"/> <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/> by way of an <see cref="AuditoryPurview"/>.
    /// </summary>
    public interface IAuditoryPerception : IAlertTypedPerception<Auditory, AuditoryPurview>
    {
        /*
         * These are copied from IOlfactoryPerception.
         * Need to come up with ones more fitting for Auditory.
         * 
        public bool AffectedByLiquidCovered { get; }

        public bool AffectedByLiquidStained { get; }

        public string InsensitiveLiquids { get; }

        public bool IsInsensitiveLiquid(string Liquid)
            => InsensitiveLiquids != null
            && (InsensitiveLiquids.CachedCommaExpansion()?.Contains(Liquid) ?? false);

        public bool IsInsensitiveLiquid(BaseLiquid Liquid)
            => Liquid != null
            && IsInsensitiveLiquid(Liquid.ID);
        */
    }
}
