using System;

using XRL.World;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class SimplePerception : BasePerception, IComposite
    {
        public const string FACE_BODYPART = "Face";

        #region Constructors

        public SimplePerception()
            : base()
        {
            Occludes = true;
            Tapers = true;
        }
        public SimplePerception(GameObject Owner, PerceptionSense Sense, int BaseScore, int BaseRadius)
            : base(Owner, Sense, BaseScore, BaseRadius)
        {
            Occludes = true;
            Tapers = true;
        }
        public SimplePerception(GameObject Owner, PerceptionSense Sense)
            : this(Owner, Sense, BASE_SCORE, BASE_RADIUS)
        {
        }

        #endregion

        public override bool Validate(GameObject Owner = null)
        {
            Owner ??= this.Owner;
            if (Owner == null)
                return false;

            if (Owner != this.Owner)
                return false;

            if (Owner.Body == null
                || Owner.Body.GetFirstPart(FACE_BODYPART, false) is null)
                return false;

            return true;
        }

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
            // Write Here.
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // Read Here.
        }

        #endregion
    }
}
