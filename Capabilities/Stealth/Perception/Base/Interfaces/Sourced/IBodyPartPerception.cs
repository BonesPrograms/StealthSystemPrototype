using System.Collections.Generic;

using XRL.World;
using XRL.World.Anatomy;

using StealthSystemPrototype.Capabilities.Stealth;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s based on the presence of a <see cref="BodyPart"/> source.
    /// </summary>
    public interface IBodyPartPerception : ISourcedPerception<BodyPart>
    {
        public string SourceType { get; }

        public new BodyPart Source { get; set; }

        public new BodyPart GetSource()
        {
            if (Owner == null
                || Owner.Body?.LoopPart(SourceType, ExcludeDismembered: true) is not List<BodyPart> bodyParts)
                return null;

            if (bodyParts.Count > 1)
                bodyParts.Sort(ClosestBodyPart);

            return bodyParts[0];
        }

        public static GameObject GetOwner(BodyPart Source)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Source?.ToString()),
                });

            return Source?.ParentBody?.ParentObject;
        }
    }
}
