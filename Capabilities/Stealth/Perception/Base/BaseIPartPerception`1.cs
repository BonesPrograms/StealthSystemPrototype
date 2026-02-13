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
        public override GameObject Owner
        {
            get => base.Owner ??= IPartPerception<T>.FindOwner(_Source);
            set => base.Owner = value;
        }

        public override T Source
        {
            get => base.Source ??= FindSource(_Owner);
            set => base.Source = value;
        }
        IPart IPartPerception.Source => Source;

        #region Constructors

        public BaseIPartPerception()
            : base()
        {
        }
        public BaseIPartPerception(
            GameObject Owner,
            T Source,
            int Level)
            : base(Owner, Source, Level)
        {
        }
        public BaseIPartPerception(
            T Source,
            int Level)
            : this(FindOwner(Source), Source, Level)
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
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(typeof(BaseIPartPerception<T>).ToStringWithGenerics()),
                });

            return GetBestSource() ?? ((ISourcedPerception<T>)this).GetSource();
        }

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
            => GetPerceiver()?.GetPartsDescendedFrom<T>();

        public virtual T GetBestSource()
            => GetPotentialSources()?.GetRandomElementCosmetic();

        public override GameObject GetPerceiver()
            => Owner;

        public new static GameObject FindOwner(T Source)
            => IPartPerception<T>.FindOwner(Source);

        public static T FindSource(GameObject Owner)
            => Owner?.GetPart<T>();

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
