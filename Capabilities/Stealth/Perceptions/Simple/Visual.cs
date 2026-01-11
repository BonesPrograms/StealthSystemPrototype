using System;

using XRL.World;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Visual : SimplePerception, IComposite
    {
        #region Constructors

        public Visual()
            : base()
        {
            Sense = PerceptionSense.Visual;
        }
        public Visual(GameObject Owner, int BaseScore, int BaseRadius)
            : base(Owner, PerceptionSense.Visual, BaseScore, BaseRadius)
        {
        }
        public Visual(GameObject Owner)
            : this(Owner, BASE_SCORE, BASE_RADIUS)
        {
        }

        #endregion

        protected override PerceptionRating? GetPerceptionRating(GameObject Owner, int? BaseScore = null, int? BaseRadius = null)
            => base.GetPerceptionRating(Owner, BaseScore, BaseRadius);

        public override int GetScore(GameObject Owner, bool ClearFirst = false)
            => base.GetScore(Owner, ClearFirst);

        public override int GetRadius(GameObject Owner, bool ClearFirst = false)
            => base.GetRadius(Owner, ClearFirst);

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
