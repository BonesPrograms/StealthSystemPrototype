using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;
using XRL.World.Parts.Skill;
using XRL.World.Parts.Mutation;
using System.Collections.Generic;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="BaseMutation"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="BaseMutation"/> source of if the underlyign <see cref="IPartPerception{BaseMutation}"/></typeparam>
    public interface IMutationPerception<T> : IMutationPerception, IPartPerception<T>
        where T : BaseMutation
    {
        public new List<T> GetPotentialSources()
            => Owner?.GetPartsDescendedFrom<T>();

        public new T GetBestSource();
    }
}