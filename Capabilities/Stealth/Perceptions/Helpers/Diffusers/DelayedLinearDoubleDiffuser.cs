using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class DelayedLinearDoubleDiffuser : LinearDoubleDiffuser
    {
        [Serializable]
        public enum DelayType : int
        {
            Steps,
            Percentage,
            ValueThreshold,
        }

        protected DelayType ValueDelayType;

        protected double Delay;

        #region Constructors

        public DelayedLinearDoubleDiffuser()
            : base()
        {
            ValueDelayType = DelayType.Steps;
            Delay = 0;
        }
        public DelayedLinearDoubleDiffuser(DelayType DelayType, double Delay)
            : this()
        {
            ValueDelayType = DelayType;
            this.Delay = Delay;
        }
        public DelayedLinearDoubleDiffuser(DelayType DelayType, double Delay, InclusiveRange Steps, double StartValue)
            : base(Steps, StartValue)
        {
            ValueDelayType = DelayType;
            this.Delay = Delay;
        }
        public DelayedLinearDoubleDiffuser(DelayType DelayType, double Delay, InclusiveRange Steps)
            : base(Steps)
        {
            ValueDelayType = DelayType;
            this.Delay = Delay;
        }

        #endregion

        protected virtual bool GetIsDelayed(double LasteValue, int Step)
            => ValueDelayType switch
            {
                DelayType.Steps => Step <= Delay,
                DelayType.Percentage => (Step * GetLinearStepValue()) <= Delay,
                DelayType.ValueThreshold => StartValue - (Step * GetLinearStepValue()) <= Delay,
                _ => false,
            };

        public override double Step(double LastValue, int Step)
            => GetIsDelayed(LastValue, Step)
            ? LastValue
            : base.Step(LastValue, Step);

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
