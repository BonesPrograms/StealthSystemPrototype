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
        public Visual(GameObject Owner, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : base(Owner, PerceptionSense.Visual, BaseDieRoll, BaseRadius)
        {
        }
        public Visual(GameObject Owner)
            : this(Owner, BASE_DIE_ROLL, new(BASE_RADIUS, VisualFlag))
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
