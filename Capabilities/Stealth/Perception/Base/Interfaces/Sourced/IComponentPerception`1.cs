using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of an <see cref="IComponent{GameObject}"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="IComponent{GameObject}"/> source of if the underlyign <see cref="ISourcedPerception{IComponent{GameObject}}"/></typeparam>
    public interface IComponentPerception<T> : ISourcedPerception<T>
        where T : IComponent<GameObject>, new()
    {
        public GameObject SourceObject { get; }

        public GameObject GetSourceObject();

        public new bool Validate()
            => ((ISourcedPerception<T>)this).Validate()
            && SourceObject != null;
    }
}
