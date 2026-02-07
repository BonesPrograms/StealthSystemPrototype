using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public class AlertCoalescer : Coalescer<IAlert>
    {
        public override IAlert CoalesceCombine(IAlert X, IAlert Y)
        {
            throw new NotImplementedException();
        }

        public override IAlert CoalesceDifference(IAlert X, IAlert Y)
        {
            throw new NotImplementedException();
        }

        public override IAlert CoalesceGreater(IAlert X, IAlert Y)
        {
            throw new NotImplementedException();
        }

        public override IAlert CoalesceLesser(IAlert X, IAlert Y)
        {
            throw new NotImplementedException();
        }
    }
}
