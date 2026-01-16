using System;

using XRL.World;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class SimplePerception : BasePerception, IComposite
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

        public override bool Validate(GameObject Owner = null)
        {
            Owner ??= this.Owner;
            if (Owner == null)
                return false;

            if (Owner != this.Owner)
                return false;

            if (Owner.Body == null
                || Owner.Body.GetFirstPart(FACE_BODYPART, false) is null)
                return false;

            return true;
        }
    }
}
