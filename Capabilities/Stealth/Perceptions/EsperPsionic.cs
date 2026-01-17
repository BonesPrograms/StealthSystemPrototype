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

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class EsperPsionic : IPartPerception<Esper>
    {
        #region Constructors

        public EsperPsionic()
            : base()
        {
            Sense = PerceptionSense.Psionic;
        }
        public EsperPsionic(GameObject Owner, Esper Source, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : base(Owner, Source, PerceptionSense.Psionic, BaseDieRoll, BaseRadius)
        {
        }
        public EsperPsionic(Esper Source, ClampedDieRoll BaseDieRoll, Radius BaseRadius)
            : this(Source?.ParentObject, Source, BaseDieRoll, BaseRadius)
        {
        }
        public EsperPsionic(Esper Source)
            : base(Source, PerceptionSense.Psionic, PsionicFlag)
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
