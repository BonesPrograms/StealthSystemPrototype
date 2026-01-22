using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Events;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public interface ISneakSource : ISneakEventHandler
    {
        public Guid SneakActivatedAbilityID { get; set; }
            
        public string  SneakActivatedAbilityClass { get; set; }

        public int BaseSneakPerformance { get; }

        public SneakPerformance SneakPerformance { get; }

        public bool IsBeingPerceived { get; }

        protected bool WantRecalc { get; set; }
    }
}
