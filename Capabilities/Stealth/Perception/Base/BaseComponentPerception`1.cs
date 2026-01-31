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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BaseComponentPerception<T>
        : BasePerception
        , IComponentPerception<T>
        where T : IComponent<GameObject>
    {
        public override GameObject Owner
        {
            get => base.Owner ??= GetOwner(Source);
            set => base.Owner = value;
        }

        protected T _Source;
        public virtual T Source
        {
            get => _Source;
            set => _Source = value;
        }

        #region Constructors

        public BaseComponentPerception()
            : base()
        {
        }
        public BaseComponentPerception(
            GameObject Owner,
            T Source,
            int Level)
            : base(Owner, Level)
        {
            this.Owner = Owner;
            this.Source = Source;
        }
        public BaseComponentPerception(
            T Source,
            int Level)
            : this(GetOwner(Source), Source, Level)
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

        public virtual T GetSource()
            => Source;

        public override GameObject GetOwner()
            => GetOwner(Source);

        public static GameObject GetOwner(T Source)
            => IComponentPerception<T>.GetOwner(Source);
    }
}
