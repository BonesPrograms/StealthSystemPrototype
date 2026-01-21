using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;

namespace StealthSystemPrototype.Senses
{
    public class SenseContext
    {
        public GameObject Owner => Perception?.Owner;

        public IPerception Perception { get; protected set; }
        public GameObject Entity { get; protected set; }

        public int Roll { get; protected set; }

        public int _Distance;
        public int Distance
        {
            get => _Distance; 
            protected set => _Distance = value;
        }

        public FindPath _PerceptionPath;
        public FindPath PerceptionPath
        {
            get => _PerceptionPath;
            protected set => _PerceptionPath = value;
        }

        public Radius Radius => Perception?.Radius;

        public double[] Diffusions { get; protected set; }
        public double Diffusion { get; protected set; }

        public bool InRadius { get; protected set; }

        protected SenseContext()
        {
            Perception = null;
            Entity = null;
            Roll = 0;
            _Distance = 0;
            PerceptionPath = null;
            InRadius = false;
            Diffusions = null;
            Diffusion = default;
        }
        public SenseContext(IPerception Perception, GameObject Entity)
            : base()
        {
            this.Perception = Perception;
            this.Entity = Entity;
            InRadius = this.Perception.CheckInRadius(Entity, out _Distance, out _PerceptionPath);
            Roll = this.Perception.Roll(Entity);
            Diffusions = Radius.Diffusions();
            Diffusion = Radius.GetDiffusion(Distance);
        }
    }
}
