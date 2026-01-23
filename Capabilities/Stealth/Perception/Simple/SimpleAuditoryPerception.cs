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
    public class SimpleAuditoryPerception : SimplePerception<Auditory>
    {
        #region Constructors

        public SimpleAuditoryPerception()
            : base()
        {
        }
        public SimpleAuditoryPerception(GameObject Owner, ClampedDieRoll BaseDieRoll, Purview BaseRadius)
            : base(Owner, BaseDieRoll, BaseRadius)
        {
        }
        public SimpleAuditoryPerception(GameObject Owner)
            : this(Owner, BASE_DIE_ROLL, new(BASE_RADIUS, AuditoryFlag))
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
