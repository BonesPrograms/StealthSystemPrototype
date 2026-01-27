using System.Collections.Generic;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="IPart"/> source.
    /// </summary>
    public interface IPartPerception : IPerception
    {
        public IPart Source { get; }

        public IPart GetSource();

        public List<IPart> GetPotentialSources();

        public IPart GetBestSource();
    }
}
