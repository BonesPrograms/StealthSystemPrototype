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
        public SimplePerception(GameObject Owner, PerceptionSense Sense, ClampedInclusiveRange BaseScore, Radius BaseRadius)
            : base(Owner, Sense, BaseScore, BaseRadius)
        {
        }
        public SimplePerception(GameObject Owner, PerceptionSense Sense)
            : this(Owner, Sense, BASE_SCORE, BASE_RADIUS)
        {
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
