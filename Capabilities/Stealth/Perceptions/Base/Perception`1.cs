using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public abstract class Perception<T>
        : BasePerception
        , IComposite
        where T
        : class,
        new()
    {
        [NonSerialized]
        protected T _Source;
        public virtual T Source => _Source ??= GetBestSource(Owner); 

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

        public abstract T GetBestSource(GameObject Owner = null);

        public override bool Validate(GameObject Owner = null)
            => (Owner ?? this.Owner) is GameObject owner
            && owner == this.Owner;

    }
}
