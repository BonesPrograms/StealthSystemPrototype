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
        public override GameObject Owner
        {
            get => base.Owner ??= GetOwner(Source);
            set => base.Owner = value;
        }

        protected string _SourceType = null;
        public virtual string SourceType
        {
            get => _SourceType;
            protected set => _SourceType = value;
        }

        protected BodyPart _Source = null;
        public virtual BodyPart Source
        {
            get
            {
                if (_Source == null)
                {
                    if (Owner?.Body?.LoopPart(SourceType, ExcludeDismembered: true) is List<BodyPart> bodyParts)
                    {
                        if (bodyParts.Count > 1)
                            bodyParts.Sort(ClosestBodyPart);

                        _Source ??= bodyParts[0];
                    }
                }
                return _Source;
            }
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
            int? PurviewValue = null)
            : base(Owner, Level)
        {
            this.Owner = Owner;
            _Source = Source;
            SourceType = Source.Type;
            Purview?.MaybeSetValue(PurviewValue);
        }
        public BaseBodyPartPerception(
            BodyPart Source,
            int Level,
            int? PurviewValue = null)
            : this(GetOwner(Source), Source, Level, PurviewValue)
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
            => Source;

        public static GameObject GetOwner(BodyPart Source)
            => IBodyPartPerception.GetOwner(Source);

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}
