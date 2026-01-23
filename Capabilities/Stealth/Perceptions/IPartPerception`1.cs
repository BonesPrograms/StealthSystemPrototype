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

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class IPartPerception<T, TSense> : Perception<T, TSense>
        where T : IPart, new()
        where TSense : ISense<TSense>, new()
    {
        public override T Source => _Source ??= GetBestSource(); 

        #region Constructors

        public IPartPerception()
            : base()
        {
        }
        public IPartPerception(
            GameObject Owner,
            T Source,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Owner, Source, BaseDieRoll, BaseRadius)
        {
        }
        public IPartPerception(
            T Source,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Source?.ParentObject, Source, BaseDieRoll, BaseRadius)
        {
        }
        public IPartPerception(
            GameObject Owner,
            T Source,
            Radius.RadiusFlags RadiusFlags = Radius.RadiusFlags.Line)
            : this(Owner, Source, BASE_DIE_ROLL, new(BASE_RADIUS, RadiusFlags))
        {
        }
        public IPartPerception(
            T Source,
            Radius.RadiusFlags RadiusFlags = Radius.RadiusFlags.Line)
            : this(Source?.ParentObject, Source, RadiusFlags)
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

        public virtual List<T> GetPotentialSources()
            => Owner?.GetPartsDescendedFrom<T>();

        public override T GetBestSource()
            => Owner
                ?.GetPart<T>()
            ?? GetPotentialSources()
                ?.GetRandomElementCosmetic();

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
    }
}
