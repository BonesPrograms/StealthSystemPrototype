using System;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public abstract class SimplePerception : IPerception
    {
        public const string FACE_BODYPART = "Face";

        #region Constructors

        public SimplePerception()
            : base()
        {
        }
        public SimplePerception(GameObject Owner, PerceptionSense Sense, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : base(Owner, Sense, BaseDieRoll, BaseRadius)
        {
        }
        public SimplePerception(GameObject Owner, PerceptionSense Sense)
            : this(Owner, Sense, BASE_DIE_ROLL, BASE_RADIUS)
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

        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            if (Owner.Body == null
                || Owner.Body.GetFirstPart(FACE_BODYPART, false) is null)
                return false;

            return true;
        }
    }
}
