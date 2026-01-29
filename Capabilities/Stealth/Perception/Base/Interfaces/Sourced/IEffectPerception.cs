using System.Collections.Generic;

using XRL.World;

using StealthSystemPrototype.Capabilities.Stealth;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="Effect"/> source.
    /// </summary>
    public interface IEffectPerception : IPerception
    {
        public Effect Source { get; }

        public Effect GetSource();

        public List<Effect> GetPotentialSources();

        public Effect GetBestSource();
    }
}
