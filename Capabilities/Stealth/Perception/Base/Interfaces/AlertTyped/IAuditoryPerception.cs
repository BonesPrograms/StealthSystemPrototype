using System.Collections.Generic;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using XRL.Liquids;
using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="Auditory"/> <see cref="IAlert"/>s contained within an <see cref="IConcealedAction"/>.
    /// </summary>
    public interface IAuditoryPerception : IAlertTypedPerception<Auditory>
    {
        public bool AffectedByLiquidCovered { get; }

        public bool AffectedByLiquidStained { get; }

        public string InsensitiveLiquids { get; }

        public bool IsInsensitiveLiquid(string Liquid)
            => InsensitiveLiquids != null
            && (InsensitiveLiquids.CachedCommaExpansion()?.Contains(Liquid) ?? false);

        public bool IsInsensitiveLiquid(BaseLiquid Liquid)
            => Liquid != null
            && IsInsensitiveLiquid(Liquid.ID);
    }
}
