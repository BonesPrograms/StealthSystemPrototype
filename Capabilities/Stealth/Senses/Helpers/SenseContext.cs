using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;
using XRL.World.Parts;

namespace StealthSystemPrototype.Senses
{
    public class SenseContext
    {
        public GameObject Perceiver => Perception?.Owner;

        public IPerception Perception { get; protected set; }

        public GameObject Hider { get; protected set; }

        public int Intensity { get; protected set; }

        public int Roll { get; protected set; }

        private int _Distance;
        public int Distance
        {
            get => _Distance; 
            protected set => _Distance = value;
        }

        private FindPath _PerceptionPath;
        public FindPath PerceptionPath
        {
            get => _PerceptionPath;
            protected set => _PerceptionPath = value;
        }

        public Radius Radius => Perception?.Radius;

        public double[] Diffusions { get; protected set; }
        public double Diffusion { get; protected set; }

        public bool InRadius { get; protected set; }

        public SneakPerformance SneakPerformance => Hider?.GetPart<UD_Sneak>()?.SneakPerformance;

        protected SenseContext()
        {
            Perception = null;
            Hider = null;
            Intensity = 0;
            Roll = 0;
            Distance = 0;
            PerceptionPath = null;
            InRadius = false;
            Diffusions = null;
            Diffusion = default;
        }
        public SenseContext(IPerception Perception, GameObject Hider)
            : base()
        {
            this.Perception = Perception;
            this.Hider = Hider;
            InRadius = this.Perception.CheckInRadius(Hider, out _Distance, out _PerceptionPath, Intensity);
            Roll = this.Perception.Roll(Hider);
            Diffusions = Radius.Diffusions();
            Diffusion = Radius.GetDiffusion(Distance);
        }
    }
}
