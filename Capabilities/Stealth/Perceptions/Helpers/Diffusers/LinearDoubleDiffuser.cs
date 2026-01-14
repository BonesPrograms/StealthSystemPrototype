using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class LinearDoubleDiffuser : BaseDoubleDiffuser
    {
        #region Constructors

        public LinearDoubleDiffuser()
            : base(0.0, new(1), 1.0)
        {
        }
        public LinearDoubleDiffuser(InclusiveRange Steps, double StartValue)
            : base(0.0, Steps, StartValue)
        {
        }
        public LinearDoubleDiffuser(InclusiveRange Steps)
            : base(0.0, Steps, 1.0)
        {
        }

        #endregion

        protected double GetLinearStepValue()
            => 1.0 / Math.Max(1, Steps.Breadth());

        public override double Step(double LastValue, int Step)
            => LastValue - GetLinearStepValue();

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
