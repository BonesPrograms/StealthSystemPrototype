using System;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class SimpleOlfactoryPerception : SimplePerception<Olfactory>
    {
        #region Constructors

        public SimpleOlfactoryPerception()
            : base()
        {
        }
        public SimpleOlfactoryPerception(GameObject Owner, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : base(Owner, BaseDieRoll, BaseRadius)
        {
        }
        public SimpleOlfactoryPerception(GameObject Owner)
            : this(Owner, BASE_DIE_ROLL, new(BASE_RADIUS, OlfactoryFlag))
        {
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        #endregion


    }
}
