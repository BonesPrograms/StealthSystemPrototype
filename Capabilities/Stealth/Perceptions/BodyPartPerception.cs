using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype.Events;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class BodyPartPerception : Perception<BodyPart>, IComposite
    {
        public string SourceType;

        #region Constructors

        public BodyPartPerception()
            : base()
        {
            SourceType = null;
        }
        public BodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            PerceptionSense Sense,
            ClampedRange BaseScore,
            Radius BaseRadius)
            : base(Owner, Source, Sense, BaseScore, BaseRadius)
        {
            SourceType = Source.Type;
        }
        public BodyPartPerception(
            BodyPart Source,
            PerceptionSense Sense,
            ClampedRange BaseScore,
            Radius BaseRadius)
            : this(Source?.ParentBody?.ParentObject, Source, Sense, BaseScore, BaseRadius)
        {
        }
        public BodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            PerceptionSense Sense)
            : this(Owner, Source, Sense, BASE_SCORE, BASE_RADIUS)
        {
        }
        public BodyPartPerception(BodyPart Source, PerceptionSense Sense)
            : this(Source, Sense, BASE_SCORE, BASE_RADIUS)
        {
        }

        #endregion

        public override BodyPart GetBestSource(GameObject Owner)
        {
            Owner ??= this.Owner;
            if (Owner == null
                || Owner.Body?.LoopPart(SourceType, ExcludeDismembered: true) is not List<BodyPart> bodyParts)
                return null;

            if (bodyParts.Count > 1)
                bodyParts.Sort(ClosestBodyPart);

            return bodyParts[0];
        }

        public override bool Validate(GameObject Owner = null)
            => (Owner ?? this.Owner) is GameObject owner
            && base.Validate(owner)
            && owner.Body != null
            && owner.Body.GetFirstPart(SourceType, false) != null;

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // do writing here
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // do reading here
        }

        #endregion
    }
}
