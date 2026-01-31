using XRL.World;

using StealthSystemPrototype.Capabilities.Stealth;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="T"/> source.
    /// </summary>
    /// <typeparam name="T">The source of if the underlyign <see cref="IPerception"/></typeparam>
    public interface ISourcedPerception<T> : IPerception
        where T : class
    {
        public T Source { get; }

        public T GetSource();

        public new bool Validate()
            => ((IPerception)this).Validate()
            && Source != null;
    }
}
