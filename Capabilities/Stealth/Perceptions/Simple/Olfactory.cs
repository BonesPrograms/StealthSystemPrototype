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
