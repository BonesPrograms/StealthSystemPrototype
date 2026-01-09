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
            : this(Owner, Sense, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
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

        protected override PerceptionRating? GetPerceptionRating(GameObject Owner = null)
            => GetPerceptionRatingEvent.GetFor(Owner, this, BaseScore, BaseRadius);

        public override int GetScore(GameObject Owner = null, bool ClearFirst = false)
            => Rating?.Score ?? 0;

        public override int GetRadius(GameObject Owner = null, bool ClearFirst = false)
            => Rating?.Radius ?? 0;

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
