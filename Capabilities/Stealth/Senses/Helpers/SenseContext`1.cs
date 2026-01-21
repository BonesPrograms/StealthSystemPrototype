using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;

namespace StealthSystemPrototype.Senses
{
    public class SenseContext<T> : SenseContext
        where T : ISense<T>, new()
    {
        public SenseContext(IPerception<T> Perception, GameObject Entity)
            : base()
        {
            this.Perception = Perception;
            this.Entity = Entity;
            InRadius = this.Perception.CheckInRadius(Entity, out _Distance, out _PerceptionPath);
            Roll = this.Perception.Roll(Entity);
        }
    }
}
