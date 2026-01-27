using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;
using XRL.World.Parts.Skill;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="BaseSkill"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="BaseSkill"/> source of if the underlyign <see cref="IPartPerception{BaseSkill}"/></typeparam>
    public interface ISkillPerception<T> : ISkillPerception, IPartPerception<T>
        where T : BaseSkill, new()
    {
    }
}
