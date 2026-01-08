using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events.Perception;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Visual : Perception, IComposite
    {
        #region Constructors
        public Visual()
            : base()
        {
        }
        public Visual(GameObject Owner, int Value, int Radius)
            : base(Owner, Value, Radius)
        {
        }
        #endregion

        public override int RollPerception()
        {
            return 0;
        }

        #region Serialization
        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // Write Here.
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // Read Here.
        }
        #endregion
    }
}
