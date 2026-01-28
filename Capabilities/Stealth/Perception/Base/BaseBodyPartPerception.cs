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
        protected string _SourceType;
        public virtual string SourceType
        {
            get => _SourceType;
            protected set => _SourceType = value;
        }

        protected BodyPart _Source;
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
            SourceType = Source.Type;
        }
        public BaseBodyPartPerception(
            BodyPart Source,
            int Level,
            IPurview Purview)
            : this(null, Source, Level, Purview)
        {
            using Indent indent = new(1);
            if (Owner == null)
            {
                Debug.LogCritical("Assigned " + nameof(Owner), Debug.CallingTypeAndMethodNames(), Indent: indent);
                Owner ??= GetOwner(Source);
            }
        }
        public BaseBodyPartPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
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

        public override void Construct()
        {
            base.Construct();
            SourceType = null;
            using Indent indent = new(1);
            if (Owner == null)
            {
                Debug.LogCritical("Assigned " + nameof(Owner), Debug.CallingTypeAndMethodNames(), Indent: indent);
                Owner ??= GetOwner(Source);
            }
        }

        public virtual BodyPart GetSource()
            => ((IBodyPartPerception)this).GetSource();

        public virtual GameObject GetOwner(BodyPart Source = null)
            => Source?.ParentBody?.ParentObject;

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}
