using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="Effect"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="Effect"/> source of if the underlyign <see cref="IComponentPerception{Effect}"/></typeparam>
    public interface IEffectPerception<T> : IEffectPerception, IComponentPerception<T>
        where T : Effect, new()
    {
    }
}
