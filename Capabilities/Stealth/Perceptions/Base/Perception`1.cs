using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class Perception<T> : IPerception
        where T : class, new()
    {
        [NonSerialized]
        protected T _Source;
        public virtual T Source => _Source ??= GetBestSource(); 

        #region Constructors

        public Perception()
            : base()
        {
            _Source = null;
        }
        public Perception(
            GameObject Owner,
            T Source,
            PerceptionSense Sense,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Owner, Sense, BaseDieRoll, BaseRadius)
        {
            _Source = Source;
        }
        public Perception(
            GameObject Owner,
            T Source,
            PerceptionSense Sense)
            : this(Owner, Source, Sense, BASE_DIE_ROLL, BASE_RADIUS)
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

        public virtual T GetBestSource()
            => null;

        public override bool Validate()
            => base.Validate();
    }
}
