using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.ZoneParts;

using StealthSystemPrototype.Coalescence;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Witnesses : CoalescibleSet<GameObject>, IComposite
    {
        #region Helpers

        protected class WitnessEqualityComparer : EqualityComparer<GameObject>
        {
            public override bool Equals(GameObject x, GameObject y)
            {
                if (x == y)
                    return true;

                if (x.ID != y.ID)
                    return false;

                return true;
            }

            public override int GetHashCode(GameObject obj)
                => obj.ID.GetHashCode();
        }
        
        protected class WitnessCoalescer : Coalescer<GameObject>
        {
            public WitnessCoalescer()
                : base(CoalesceMethod.First)
            { }
            public WitnessCoalescer(CoalesceMethod CoalesceMethod)
                : base(CoalesceMethod)
            { }

            public override GameObject CoalesceFirst(GameObject x, GameObject y)
                => x;

            public override GameObject CoalesceSecond(GameObject x, GameObject y)
                => y;

            public override GameObject CoalesceGreater(GameObject x, GameObject y)
                => throw Nonsense_NotSupportedException();

            public override GameObject CoalesceLesser(GameObject x, GameObject y)
                => throw Nonsense_NotSupportedException();

            public override GameObject CoalesceCombine(GameObject x, GameObject y)
                => throw Nonsense_NotSupportedException();

            public override GameObject CoalesceDifference(GameObject x, GameObject y)
                => throw Nonsense_NotSupportedException();
        }

        #endregion

        protected static WitnessEqualityComparer DefaultEqualityComparer => new();

        protected static WitnessCoalescer DefaultCoalescer => new();

        private UD_SneakWitnesser _ParentPart;
        public UD_SneakWitnesser ParentPart
        {
            get => _ParentPart;
            protected set => _ParentPart = value;
        }

        public Witnesses()
            : base()
        {
            ParentPart = null;
        }
        public Witnesses(UD_SneakWitnesser ParentPart, IReadOnlyList<GameObject> List)
            : base(List, DefaultEqualityComparer, DefaultCoalescer)
        {
            this.ParentPart = ParentPart;
        }
        public Witnesses(UD_SneakWitnesser ParentPart)
            : this(ParentPart, null)
        { }
        public Witnesses(Witnesses Source)
            : this(Source?.ParentPart, Source?.ToList())
        { }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
        }

        #endregion
    }
}
