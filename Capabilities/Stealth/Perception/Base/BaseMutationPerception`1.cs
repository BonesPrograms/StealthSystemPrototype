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
using XRL.World.Parts;

namespace StealthSystemPrototype.Perceptions
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BaseMutationPerception<T>
        : BaseIPartPerception<T>
        , IMutionPerception<T>
        where T : BaseMutation
    {
        BaseMutation IMutionPerception.Source => Source;

        #region Constructors

        public BaseMutationPerception()
            : base()
        {
        }
        public BaseMutationPerception(
            GameObject Owner,
            T Source,
            int Level,
            IPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
        }
        public BaseMutationPerception(
            T Source,
            int Level,
            IPurview Purview)
            : base(Source, Level, Purview)
        {
        }
        public BaseMutationPerception(GameObject Basis, SerializationReader Reader)
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

        public override T GetSource()
            => GetBestSource()
            ?? GetPotentialSources()
                ?.GetRandomElementCosmetic()
            ?? base.GetSource();

        public override bool Validate()
            => !GetPotentialSources().IsNullOrEmpty()
            && base.Validate();

        public override List<T> GetPotentialSources()
            => Owner
                ?.GetPart< Mutations>()
                ?.ActiveMutationList
                ?.Where(bm => bm.GetType().InheritsFrom(typeof(T)))
                ?.Select(bm => bm as T)
                ?.ToList() is List<T> mutationsList
            ? mutationsList
            : null;

        public override T GetBestSource()
            => GetPotentialSources()
                ?.Where(bm => bm.Level > 0)
                ?.OrderBy(keySelector: bm => -bm.Level)
                ?.FirstOrDefault();

        public override GameObject GetOwner(T Source = null)
            => base.GetOwner(Source);

        #region Explicit Implementations

        List<BaseMutation> IMutionPerception.GetPotentialSources()
            => GetPotentialSources()?.ConvertAll(p => p as BaseMutation);

        BaseMutation IMutionPerception.GetSource()
            => GetSource();

        BaseMutation IMutionPerception.GetBestSource()
            => GetBestSource();

        #endregion
    }
}
