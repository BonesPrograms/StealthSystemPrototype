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
        public Auditory(GameObject Owner, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : base(Owner, PerceptionSense.Auditory, BaseDieRoll, BaseRadius)
        {
        }
        public Auditory(GameObject Owner)
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
