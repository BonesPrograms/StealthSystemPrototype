using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Coalescence;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public class AlertCoalescer : Coalescer<IAlert>
    {
        public AlertCoalescer()
            : base(CoalesceMethod.Combine)
        { }
        public AlertCoalescer(CoalesceMethod CoalesceMethod)
            : base(CoalesceMethod)
        { }

        public override IAlert CoalesceFirst(IAlert x, IAlert y)
            => x;

        public override IAlert CoalesceSecond(IAlert x, IAlert y)
            => y;

        public override IAlert CoalesceGreater(IAlert x, IAlert y)
            => y.Intensity > x.Intensity
            ? y
            : x;

        public override IAlert CoalesceLesser(IAlert x, IAlert y)
            => y.Intensity < x.Intensity
            ? y
            : x;

        public override IAlert CoalesceCombine(IAlert x, IAlert y)
            => x.AdjustIntensity(y.Intensity);

        public override IAlert CoalesceDifference(IAlert x, IAlert y)
            => x.AdjustIntensity(-y.Intensity);

        public override IAlert CoalesceTypeDefined(IAlert x, IAlert y)
            => x.Coalesce(y);
    }
}
