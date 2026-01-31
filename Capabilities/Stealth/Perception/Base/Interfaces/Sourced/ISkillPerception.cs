using System.Collections.Generic;

using XRL.World;
using XRL.World.Parts.Skill;

using StealthSystemPrototype.Capabilities.Stealth;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="BaseSkill"/> source.
    /// </summary>
    public interface ISkillPerception : IPartPerception
    {
        public new BaseSkill Source { get; }

        public new BaseSkill GetSource();

        public new List<BaseSkill> GetPotentialSources();

        public new BaseSkill GetBestSource();
    }
}
