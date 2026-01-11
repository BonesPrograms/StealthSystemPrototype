using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class Perception<T>
        : BasePerception
        , IComposite
        where T
        : class,
        new()
    {
        [NonSerialized]
        protected T _Source;
        public virtual T Source => _Source ??= GetBestSource(Owner); 

        #region Constructors

        public Perception()
            : base()
        {
            _Source = null;
        }
        public Perception(GameObject Owner, T Source, PerceptionSense Sense, int BaseScore, int BaseRadius)
            : base(Owner, Sense, BaseScore, BaseRadius)
        {
            _Source = Source;
        }
        public Perception(GameObject Owner, T Source, PerceptionSense Sense)
            : this(Owner, Source, Sense, BASE_SCORE, BASE_RADIUS)
        {
        }

        #endregion

        public abstract T GetBestSource(GameObject Owner = null);

        public override bool Validate(GameObject Owner = null)
            => (Owner ?? this.Owner) is GameObject owner
            && owner == this.Owner;

        protected override PerceptionRating? GetPerceptionRating(
            GameObject Owner = null,
            int? BaseScore = null,
            int? BaseRadius = null)
            => GetPerceptionScoreEvent.GetFor(
                    Perceiver: Owner,
                    Perception: this,
                    Rating: base.GetPerceptionRating(
                        Owner: Owner,
                        BaseScore: BaseScore ?? this.BaseScore,
                        BaseRadius: BaseRadius ?? this.BaseRadius));

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
