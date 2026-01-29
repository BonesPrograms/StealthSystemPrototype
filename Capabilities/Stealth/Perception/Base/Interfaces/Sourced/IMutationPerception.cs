using System.Collections.Generic;

using XRL.World;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype.Capabilities.Stealth;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="BaseMutation"/> source.
    /// </summary>
    public interface IMutationPerception : IPartPerception
    {
        public new BaseMutation Source { get; }

        public new BaseMutation GetSource();

        public new List<BaseMutation> GetPotentialSources();

        public new BaseMutation GetBestSource();
    }
}
