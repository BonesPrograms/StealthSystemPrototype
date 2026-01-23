using System;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Senses;

using static StealthSystemPrototype.Const;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public abstract class SimplePerception<TSense> : IPerception<TSense>
        where TSense : ISense<TSense>, new()
    {
        #region Constructors

        public SimplePerception()
            : base()
        {
        }
        public SimplePerception(
            GameObject Owner,
            ClampedDieRoll BaseDieRoll,
            Purview BaseRadius)
            : base(Owner, BaseDieRoll, BaseRadius)
        {
        }
        public SimplePerception(GameObject Owner)
            : this(Owner, BASE_DIE_ROLL, BASE_RADIUS)
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
