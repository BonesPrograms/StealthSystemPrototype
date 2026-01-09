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
            : this(Owner, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
        {
        }

        #endregion

        protected override PerceptionRating? GetPerceptionRating(GameObject Owner = null)
            => base.GetPerceptionRating(Owner);

        public override int GetScore(GameObject Owner = null, bool ClearFirst = false)
            => base.GetScore(Owner, ClearFirst);

        public override int GetRadius(GameObject Owner = null, bool ClearFirst = false)
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
