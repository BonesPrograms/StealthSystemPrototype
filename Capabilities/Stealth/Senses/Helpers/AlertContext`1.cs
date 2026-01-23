using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;

namespace StealthSystemPrototype.Senses
{
    public class AlertContext<TAlert> : AlertContext
        where TAlert : IAlert, new()
    {
        public IPerception<TAlert> TypedPerception
        {
            get => Perception as IPerception<TAlert>;
            protected set => Perception = value;
        }

        public AlertContext(int Intensity, IPerception<TAlert> Perception, GameObject Hider)
            : base(Perception, Hider)
        {
            this.Intensity = Intensity;
        }
        public AlertContext(AlertContext Source)
            : base(Source)
        {
        }
        public AlertContext(AlertContext<TAlert> Source)
            : this(Source as AlertContext)
        {
        }
    }
}
