using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Coalescence;

using XRL.Collections;
using XRL.World;
using XRL.World.ZoneParts;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Sneakers : CoalescibleSet<ISneakSource>, IComposite
    {
        #region Helpers

        protected class SneakSourceEqualityComparer : EqualityComparer<ISneakSource>
        {
            public override bool Equals(ISneakSource x, ISneakSource y)
                => x == y || x.Sneaker == y.Sneaker;

            public override int GetHashCode(ISneakSource obj)
                => obj.Sneaker.ID.GetHashCode();
        }

        protected class SneakSourceCoalescer : Coalescer<ISneakSource>
        {
            public SneakSourceCoalescer()
                : base(CoalesceMethod.Greater)
            { }
            public SneakSourceCoalescer(CoalesceMethod CoalesceMethod)
                : base(CoalesceMethod)
            { }

            public override ISneakSource CoalesceFirst(ISneakSource x, ISneakSource y)
                => x;

            public override ISneakSource CoalesceSecond(ISneakSource x, ISneakSource y)
                => y;

            public override ISneakSource CoalesceGreater(ISneakSource x, ISneakSource y)
                => new SneakPerformanceComparer().Compare(y.SneakPerformance, x.SneakPerformance) < 0
                ? y
                : x;

            public override ISneakSource CoalesceLesser(ISneakSource x, ISneakSource y)
                => new SneakPerformanceComparer().Compare(y.SneakPerformance, x.SneakPerformance) > 0
                ? y
                : x;

            public override ISneakSource CoalesceCombine(ISneakSource x, ISneakSource y)
                => throw Nonsense_NotSupportedException();

            public override ISneakSource CoalesceDifference(ISneakSource x, ISneakSource y)
                => throw Nonsense_NotSupportedException();
        }

        #endregion

        protected static SneakSourceEqualityComparer DefaultEqualityComparer => new();

        protected static SneakSourceCoalescer DefaultCoalescer => new();

        private UD_SneakWitnesser _ParentPart;
        public UD_SneakWitnesser ParentPart
        {
            get => _ParentPart;
            protected set => _ParentPart = value;
        }

        public Sneakers()
            : base()
        {
            ParentPart = null;
        }
        public Sneakers(UD_SneakWitnesser ParentPart, IReadOnlyList<ISneakSource> List)
            : base(List, DefaultEqualityComparer, DefaultCoalescer)
        {
            this.ParentPart = ParentPart;
        }
        public Sneakers(UD_SneakWitnesser ParentPart)
            : this(ParentPart, null)
        { }
        public Sneakers(Sneakers Source)
            : this(Source?.ParentPart, Source?.ToList())
        { }
    }
}
