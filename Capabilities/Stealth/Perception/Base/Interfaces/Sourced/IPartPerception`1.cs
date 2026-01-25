using System.Collections.Generic;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Detetections;
using XRL.World;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="IPart"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="IPart"/> source of if the underlyign <see cref="IComponentPerception{IPart}"/></typeparam>
    public interface IPartPerception<T> : IPartPerception, IComponentPerception<T>
        where T : IPart
    {
        /*
        public new T Source { get; }

        public new T GetSource()
            => Owner?.GetPart<T>();
        */
        public new List<T> GetPotentialSources()
            => Owner?.GetPartsDescendedFrom<T>();

        public new T GetBestSource();

        /*
        public new bool Validate()
            => ((ISourcedPerception<T>)this).Validate()
            && Source != null;
        */
    }
}
