using System;

using XRL.World;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Olfactory : SimplePerception, IComposite
    {
        #region Constructors

        public Olfactory()
            : base()
        {
            Sense = PerceptionSense.Olfactory;
        }
        public Olfactory(GameObject Owner, ClampedRange BaseScore, Radius BaseRadius)
            : base(Owner, PerceptionSense.Olfactory, BaseScore, BaseRadius)
        {
        }
        public Olfactory(GameObject Owner)
            : this(Owner, BASE_SCORE, new(BASE_RADIUS, Radius.RadiusFlags.Pathing ^ Radius.RadiusFlags.Area ^ Radius.RadiusFlags.Tapers))
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
