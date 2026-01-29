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
    public abstract class BaseIPartPerception<T>
        : BaseComponentPerception<T>
        , IPartPerception<T>
        where T : IPart
    {
        IPart IPartPerception.Source => Source;

        #region Constructors

        public BaseIPartPerception()
            : base()
        {
        }
        public BaseIPartPerception(
            GameObject Owner,
            T Source,
            int Level,
            IPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
        }
        public BaseIPartPerception(
            T Source,
            int Level,
            IPurview Purview)
            : base(Source, Level, Purview)
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

        public override T GetSource()
            => GetBestSource() ?? ((ISourcedPerception<T>)this).GetSource();

        public override bool Validate()
        {
            if (!base.Validate())
                return false;

            if (Source != null
                && !Owner.HasPart(Source?.Name))
                _Source = null;

            if (Source == null)
                return false;

            return Owner.HasPart<T>();
        }

        public virtual List<T> GetPotentialSources()
            => ((IPartPerception<T>)this).GetPotentialSources();

        public abstract T GetBestSource();

        public override GameObject GetOwner(T Source = null)
            => (this.Source ??= Source ?? this.Source)?.ParentObject;

        #region Explicit Implementations

        IPart IPartPerception.GetSource()
            => GetSource();

        List<IPart> IPartPerception.GetPotentialSources()
            => GetPotentialSources()?.ConvertAll(p => p as IPart);

        IPart IPartPerception.GetBestSource()
            => GetBestSource();

        #endregion
    }
}
