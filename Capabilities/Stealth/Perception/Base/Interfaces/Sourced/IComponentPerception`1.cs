using XRL.World;

using StealthSystemPrototype.Capabilities.Stealth;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of an <see cref="IComponent{GameObject}"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="IComponent{GameObject}"/> source of if the underlyign <see cref="ISourcedPerception{IComponent{GameObject}}"/></typeparam>
    public interface IComponentPerception<T> : ISourcedPerception<T>
        where T : IComponent<GameObject>
    {
    }
}
