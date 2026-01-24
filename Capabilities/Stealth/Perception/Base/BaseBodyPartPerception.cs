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
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BaseBodyPartPerception<TAlert> : BasePerception, IBodyPartPerception
        where TAlert : class, IAlert, new()
    {
        private string _SourceType;
        public string SourceType
        {
            get => _SourceType;
            protected set => _SourceType = value;
        }

        private BodyPart _Source;
        public BodyPart Source
        {
            get => _Source ??= GetSource();
            set => _Source = value;
        }

        #region Constructors

        public BaseBodyPartPerception()
            : base()
        {
            SourceType = null;
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
            : this(Source?.ParentBody?.ParentObject, Source, Level, Purview)
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
            => ((IBodyPartPerception)this).GetSource();

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;

    }
}
