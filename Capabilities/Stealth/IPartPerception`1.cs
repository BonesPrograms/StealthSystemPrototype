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
        public IPartPerception(GameObject Owner, T Source, PerceptionSense Sense, int BaseScore, int BaseRadius)
            : base(Owner, Source, Sense, BaseScore, BaseRadius)
        {
        }
        public IPartPerception(GameObject Owner, T Source, PerceptionSense Sense)
            : this(Owner, Source, Sense, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
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
            if (Owner == null)
                return false;

            if (Owner != this.Owner)
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
