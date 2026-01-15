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
        public Olfactory(GameObject Owner, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : base(Owner, PerceptionSense.Olfactory, BaseDieRoll, BaseRadius)
        {
        }
        public Olfactory(GameObject Owner)
            : this(Owner, BASE_DIE_ROLL, new(BASE_RADIUS, OlfactoryFlag))
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
