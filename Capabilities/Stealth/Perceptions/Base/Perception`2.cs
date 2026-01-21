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
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public abstract class Perception<T, TSense> : IPerception<TSense>
        where T : class, new()
        where TSense : ISense<TSense>, new()
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
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Owner, BaseDieRoll, BaseRadius)
        {
            _Source = Source;
        }
        public Perception(
            GameObject Owner,
            T Source)
            : this(Owner, Source, BASE_DIE_ROLL, BASE_RADIUS)
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
