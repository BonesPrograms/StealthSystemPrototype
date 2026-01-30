using System.Collections.Generic;

using XRL.World;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="IPart"/> source.
    /// </summary>
    /// <typeparam name="T">The <see cref="IPart"/> source of if the underlyign <see cref="IComponentPerception{IPart}"/></typeparam>
    public interface IPartPerception<T> : IPartPerception, IComponentPerception<T>
        where T : IPart
    {
        public new List<T> GetPotentialSources()
            => Owner?.GetPartsDescendedFrom<T>();

        public new T GetBestSource();

        public new static GameObject GetOwner(T Source)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Source?.ToString()),
                });

            return Source?.ParentObject;
        }
    }
}
