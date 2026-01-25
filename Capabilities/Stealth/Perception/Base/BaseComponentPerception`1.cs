using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Senses;
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
        protected T _Source;
        public virtual T Source
        {
            get => _Source ??= GetSource();
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
            int Level,
            IPurview Purview)
            : base(Owner, Level, Purview)
        {
            this.Source = Source;
        }
        public BaseComponentPerception(
            T Source,
            int Level,
            IPurview Purview)
            : this(null, Source, Level, Purview)
        {
            Owner ??= GetOwner(Source);
        }
        public BaseComponentPerception(GameObject Basis, SerializationReader Reader)
            : base(Basis, Reader)
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

        public override void Construct()
        {
            base.Construct();
            Owner ??= GetOwner(Source);
            Source = null;
        }

        public abstract T GetSource();

        public abstract GameObject GetOwner(T Source = null);
    }
}
