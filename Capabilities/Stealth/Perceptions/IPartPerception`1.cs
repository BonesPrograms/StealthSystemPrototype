using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.Rules;
using XRL.World;
using XRL.World.Parts.Mutation;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class IPartPerception<T>
        : Perception<T>
        , IComposite
        where T
        : IPart,
        new()
    {
        public override T Source => _Source ??= GetBestSource(Owner); 

        #region Constructors

        public IPartPerception()
            : base()
        {
        }
        public IPartPerception(
            GameObject Owner,
            T Source,
            PerceptionSense Sense,
            ClampedBreadth BaseScore,
            Radius BaseRadius)
            : base(Owner, Source, Sense, BaseScore, BaseRadius)
        {
        }
        public IPartPerception(
            T Source,
            PerceptionSense Sense,
            ClampedBreadth BaseScore,
            Radius BaseRadius)
            : base(Source?.ParentObject, Source, Sense, BaseScore, BaseRadius)
        {
        }
        public IPartPerception(
            GameObject Owner,
            T Source,
            PerceptionSense Sense,
            Radius.RadiusFlags RadiusFlags = Radius.RadiusFlags.Line)
            : this(Owner, Source, Sense, BASE_SCORE, new(BASE_RADIUS, RadiusFlags))
        {
        }
        public IPartPerception(
            T Source,
            PerceptionSense Sense,
            Radius.RadiusFlags RadiusFlags = Radius.RadiusFlags.Line)
            : this(Source?.ParentObject, Source, Sense, RadiusFlags)
        {
        }

        #endregion

        public virtual List<T> GetPotentialSources(GameObject Owner = null)
            => (Owner ?? this.Owner)?.GetPartsDescendedFrom<T>();

        public override T GetBestSource(GameObject Owner = null)
            => (Owner ?? this.Owner)
                ?.GetPart<T>()
            ?? GetPotentialSources(Owner)
                ?.GetRandomElementCosmetic();

        public override bool Validate(GameObject Owner = null)
        {
            Owner ??= this.Owner;
            if (!base.Validate(Owner))
                return false;

            if (Source != null
                && !Owner.HasPart(Source?.Name))
                _Source = null;

            if (Source == null)
                return false;

            return Owner.HasPart<T>();
        }

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
