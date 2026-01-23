using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.Parts;
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
    public class EsperPsionicPerception : IPartPerception<Esper, Psionic>
    {
        #region Constructors

        public EsperPsionicPerception()
            : base()
        {
        }
        public EsperPsionicPerception(GameObject Owner, Esper Source, ClampedDieRoll BaseDieRoll, Purview BaseRadius)
            : base(Owner, Source, BaseDieRoll, BaseRadius)
        {
        }
        public EsperPsionicPerception(Esper Source, ClampedDieRoll BaseDieRoll, Purview BaseRadius)
            : this(Source?.ParentObject, Source, BaseDieRoll, BaseRadius)
        {
        }
        public EsperPsionicPerception(Esper Source)
            : base(Source, PsionicFlag)
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

        public override int GetBonusBaseDieRoll()
            => base.GetBonusBaseDieRoll()
            + (Owner?.GetPart<Mutations>()
                ?.MutationList
                ?.Where(bm => bm.IsMental())
                ?.OrderInPlace((bm1, bm2) => bm1.Level.CompareTo(bm2.Level))
                ?.FirstOrDefault()
                ?.Level
                ?? 0);

        public override int GetBonusBaseRadius()
            => base.GetBonusBaseDieRoll()
            + Math.Min(Owner?.StatMod("Ego") ?? 0, 10);
    }
}
