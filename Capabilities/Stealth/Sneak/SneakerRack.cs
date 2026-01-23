using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class SneakerRack : Rack<ISneakSource>
    {
        public GameObject Perceiver;

        protected SneakerRack()
            : base()
        {
        }

        public SneakerRack(GameObject Perceiver)
            : this()
        {
            this.Perceiver = Perceiver;
        }
    }
}
