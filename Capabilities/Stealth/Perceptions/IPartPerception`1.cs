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
    public class IPartPerception<T, S> : Perception<T, S>
        where T : IPart, new()
        where S : ISense, new()
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
            PerceptionSense Sense,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Owner, Source, Sense, BaseDieRoll, BaseRadius)
        {
        }
        public IPartPerception(
            T Source,
            PerceptionSense Sense,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Source?.ParentObject, Source, Sense, BaseDieRoll, BaseRadius)
        {
        }
        public IPartPerception(
            GameObject Owner,
            T Source,
            PerceptionSense Sense,
            Radius.RadiusFlags RadiusFlags = Radius.RadiusFlags.Line)
            : this(Owner, Source, Sense, BASE_DIE_ROLL, new(BASE_RADIUS, RadiusFlags))
        {
        }
        public IPartPerception(
            T Source,
            PerceptionSense Sense,
            Radius.RadiusFlags RadiusFlags = Radius.RadiusFlags.Line)
            : this(Source?.ParentObject, Source, Sense, RadiusFlags)
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
