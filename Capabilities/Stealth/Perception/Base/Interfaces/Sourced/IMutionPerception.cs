using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;
using XRL.World.Parts.Skill;
using XRL.World.Parts.Mutation;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="BaseMutation"/> source.
    /// </summary>
    public interface IMutionPerception : IPartPerception
    {
        public new BaseMutation Source { get; }

        public new BaseMutation GetSource();
    }
}
