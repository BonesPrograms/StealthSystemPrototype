using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public interface ISneakSource : ISneakPerformanceEventHandler
    {
        public GameObject Sneaker { get; }

        public int BaseSneakPerformance { get; }

        public SneakPerformance SneakPerformance { get; }

        public Guid SneakActivatedAbilityID { get; set; }

        public string SneakActivatedAbilityName { get; }

        public string SneakActivatedAbilityCommand { get; }

        public string SneakSourceDescription { get; }
            
        public string SneakActivatedAbilityClass { get; }
            
        public bool SneakActivatedAbilityIsRealityDistortionBased { get; }

        public bool SneakBeingPerceived { get; }

        public bool SneakWantRecalc { get; set; }

        public bool SneakSneaking { get; set; }
    }
}
