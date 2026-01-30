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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BaseBodyPartPerception
        : BasePerception
        , IBodyPartPerception
    {
        protected string _SourceType = null;
        public virtual string SourceType
        {
            get => _SourceType;
            protected set => _SourceType = value;
        }

        protected BodyPart _Source = null;
        public virtual BodyPart Source
        {
            get => _Source ??= GetSource();
            set => _Source = value;
        }

        #region Constructors

        public BaseBodyPartPerception()
            : base()
        {
        }
        public BaseBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            IPurview Purview)
            : base(Owner, Level, Purview)
        {
            this.Owner = Owner;
            _Source = Source;
            SourceType = Source.Type;
        }
        public BaseBodyPartPerception(
            BodyPart Source,
            int Level,
            IPurview Purview)
            : this(GetOwner(Source), Source, Level, Purview)
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

        public virtual BodyPart GetSource()
        {
            if (Owner == null
                || Owner.Body?.LoopPart(SourceType, ExcludeDismembered: true) is not List<BodyPart> bodyParts)
                return null;

            if (bodyParts.Count > 1)
                bodyParts.Sort(ClosestBodyPart);

            return bodyParts[0];
        }

        public virtual GameObject GetOwner()
            => GetOwner(Source);

        public static GameObject GetOwner(BodyPart Source)
            => IBodyPartPerception.GetOwner(Source);

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}
