using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class ExponentialDoubleDiffuser : BaseDoubleDiffuser
    {
        #region Constructors

        public ExponentialDoubleDiffuser()
            : base()
        {
        }
        public ExponentialDoubleDiffuser(InclusiveRange Steps, double StartValue)
            : base(Steps, StartValue)
        {
        }
        public ExponentialDoubleDiffuser(InclusiveRange Steps)
            : base(0.0, Steps, 1.0)
        {
        }

        #endregion

        public override double Step(double LastValue, int Step)
            => Math.Pow(Math.Pow(Amount, Step), 2.0 - LastValue);

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);

            // do writing here
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // do reading here
        }

        #endregion
    }
}
