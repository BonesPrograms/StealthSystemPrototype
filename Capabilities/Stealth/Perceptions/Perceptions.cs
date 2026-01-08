using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public partial class Perceptions : IComposite
    {
        private Perception[] Items;


        #region Serialization
        public void Write(SerializationWriter Writer)
        {
        }

        public void Read(SerializationReader Reader)
        {
        }
        #endregion
    }
}
