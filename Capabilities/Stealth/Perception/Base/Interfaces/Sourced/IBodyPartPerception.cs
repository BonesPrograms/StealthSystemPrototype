using System.Collections.Generic;

using XRL.World.Anatomy;

using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s based on the presence of a <see cref="BodyPart"/> source.
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
    }
}
