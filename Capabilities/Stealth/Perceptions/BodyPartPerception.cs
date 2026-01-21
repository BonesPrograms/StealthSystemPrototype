using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class BodyPartPerception<TSense> : Perception<BodyPart, TSense>
        where TSense : ISense<TSense>, new()
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
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Owner, Source, BaseDieRoll, BaseRadius)
        {
            SourceType = Source.Type;
        }
        public BodyPartPerception(
            BodyPart Source,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : this(Source?.ParentBody?.ParentObject, Source, BaseDieRoll, BaseRadius)
        {
        }
        public BodyPartPerception(
            GameObject Owner,
            BodyPart Source)
            : this(Owner, Source, BASE_DIE_ROLL, BASE_RADIUS)
        {
        }
        public BodyPartPerception(BodyPart Source)
            : this(Source, BASE_DIE_ROLL, BASE_RADIUS)
        {
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        #endregion

        public override BodyPart GetBestSource()
        {
            if (Owner == null
                || Owner.Body?.LoopPart(SourceType, ExcludeDismembered: true) is not List<BodyPart> bodyParts)
                return null;

            if (bodyParts.Count > 1)
                bodyParts.Sort(ClosestBodyPart);

            return bodyParts[0];
        }

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;

    }
}
