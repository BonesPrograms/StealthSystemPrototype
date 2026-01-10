using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class EsperPsionic : IPartPerception<Esper>, IComposite
    {
        #region Constructors

        public EsperPsionic()
            : base()
        {
            Occludes = true;
            Tapers = true;
            Sense = PerceptionSense.Psionic;
        }
        public EsperPsionic(GameObject Owner, Esper Source, int BaseScore, int BaseRadius)
            : base(Owner, Source, PerceptionSense.Psionic, BaseScore, BaseRadius)
        {
            Occludes = true;
            Tapers = true;
        }
        public EsperPsionic(Esper Source, int BaseScore, int BaseRadius)
            : this(Source?.ParentObject, Source, BaseScore, BaseRadius)
        {
        }
        public EsperPsionic(Esper Source)
            : this(Source, BASE_PERCEPTION_SCORE, BASE_PERCEPTION_RADIUS)
        {
        }

        #endregion

        public override int GetBonusBaseScore()
            => base.GetBonusBaseScore()
            + (Owner?.GetPart<Mutations>()
                ?.MutationList
                ?.Where(bm => bm.IsMental())
                ?.OrderInPlace((bm1, bm2) => bm1.Level.CompareTo(bm2.Level))
                ?.FirstOrDefault()
                ?.Level
                ?? 0);

        public override int GetBonusBaseRadius()
            => base.GetBonusBaseScore()
            + Math.Min(Owner?.StatMod("Ego") ?? 0, 10);

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // Write Here.
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // Read Here.
        }

        #endregion
    }
}
