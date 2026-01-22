using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;

namespace StealthSystemPrototype.Senses
{
    public class SenseContext<TSense> : SenseContext
        where TSense : ISense<TSense>, new()
    {
        public IPerception<TSense> TypedPerception
        {
            get => Perception as IPerception<TSense>;
            protected set => Perception = value;
        }

        public SenseContext(int Intensity, IPerception<TSense> Perception, GameObject Hider)
            : base(Perception, Hider)
        {
            this.Intensity = Intensity;
        }
    }
}
