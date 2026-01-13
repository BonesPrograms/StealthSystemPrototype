using System;

using XRL.World;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Auditory : SimplePerception, IComposite
    {
        #region Constructors

        public Auditory()
            : base()
        {
            Sense = PerceptionSense.Auditory;
        }
        public Auditory(GameObject Owner, ClampedRange BaseScore, Radius BaseRadius)
            : base(Owner, PerceptionSense.Auditory, BaseScore, BaseRadius)
        {
        }
        public Auditory(GameObject Owner)
            : this(Owner, BASE_SCORE, new(BASE_RADIUS, AuditoryFlag))
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
