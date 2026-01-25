using System.Collections.Generic;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;
using XRL.World.Parts.Skill;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="BaseSkill"/> source.
    /// </summary>
    public interface ISkillPerception : IPartPerception
    {
        public new BaseSkill Source { get; }

        public new BaseSkill GetSource();

        public new List<BaseSkill> GetPotentialSources();

        public new BaseSkill GetBestSource();
    }
}
