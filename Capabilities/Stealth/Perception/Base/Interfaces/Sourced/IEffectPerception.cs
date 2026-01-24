using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="Effect"/> source.
    /// </summary>
    public interface IEffectPerception : IPerception
    {
        public Effect Source { get; }

        public GameObject SourceObject { get; }

        public Effect GetSource();

        public GameObject GetSourceObject();
    }
}
