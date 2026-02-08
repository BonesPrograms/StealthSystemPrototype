using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class SneakerSet : HashSet<ISneakSource>
    {
        public GameObject Perceiver;

        protected SneakerSet()
            : base()
        {
        }

        public SneakerSet(GameObject Perceiver)
            : this()
        {
            this.Perceiver = Perceiver;
        }
    }
}
