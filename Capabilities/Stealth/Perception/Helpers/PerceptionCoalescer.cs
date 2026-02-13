using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Coalescence;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class PerceptionCoalescer : Coalescer<IPerception>
    {
        public PerceptionCoalescer()
            : base(CoalesceMethod.Greater)
        { }

        public PerceptionCoalescer(CoalesceMethod CoalesceMethod)
            : base(CoalesceMethod)
        { }

        public override IPerception CoalesceFirst(IPerception x, IPerception y)
            => x;

        public override IPerception CoalesceSecond(IPerception x, IPerception y)
            => y;

        public override IPerception CoalesceGreater(IPerception x, IPerception y)
            => new PerceptionComparer().Compare(y, x) > 0
            ? y
            : x
            ;

        public override IPerception CoalesceLesser(IPerception x, IPerception y)
            => new PerceptionComparer().Compare(y, x) < 0
            ? y
            : x
            ;

        public override IPerception CoalesceCombine(IPerception x, IPerception y)
            => throw Nonsense_NotSupportedException();

        public override IPerception CoalesceDifference(IPerception x, IPerception y)
            => throw Nonsense_NotSupportedException();
    }
}
