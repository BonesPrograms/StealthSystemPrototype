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
        public Visual(GameObject Owner, ClampedInclusiveRange BaseScore, Radius BaseRadius)
            : base(Owner, PerceptionSense.Visual, BaseScore, BaseRadius)
        {
        }
        public Visual(GameObject Owner)
            : this(Owner, BASE_SCORE, new(BASE_RADIUS, VisualFlag))
        {
        }

        #endregion
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
