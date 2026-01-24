using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;

namespace StealthSystemPrototype.Alerts
{
    public class AlertContext<P, A> : AlertContext
        where P : class, IAlertTypedPerception<A>, new()
        where A : class, IAlert, new()
    {
        public P TypedPerception
        {
            get => Perception as P;
            protected set => Perception = value;
        }

        protected AlertContext()
            : base()
        {
        }
        public AlertContext(
            IConcealedAction ParentAction,
            P Perception,
            A Alert,
            int Intensity,
            GameObject Actor,
            Cell AlertLocation)
            : base(
                  ParentAction: ParentAction,
                  Perception: Perception,
                  Alert: Alert,
                  Intensity: Intensity,
                  Actor: Actor,
                  AlertLocation: AlertLocation)
        {
        }
        public AlertContext(AlertContext Source)
            : base(Source)
        {
        }
        public AlertContext(AlertContext<P, A> Source)
            : base(Source)
        {
        }
    }
}
