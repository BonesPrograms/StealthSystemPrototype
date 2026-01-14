using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public class DoubleDiffuser : SerializableSequence<double>
    {
        public double Amount;

        #region Constructors

        public DoubleDiffuser(double Amount, double StartValue, InclusiveRange Steps)
            : base(StartValue, Steps)
        {
            this.Amount = Amount;
        }

        #endregion

        public override double Step(double LastValue, int Step)
            => Math.Pow(Amount, Step) * LastValue;

        #region Serialization

        public override void WriteStartValue(SerializationWriter Writer, double StartValue)
        {
            Writer.Write(StartValue);
        }
        public override void ReadStartValue(SerializationReader Reader, out double StartValue)
        {
            StartValue = Reader.ReadDouble();
        }

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);

            Writer.Write(Amount);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);

            Amount = Reader.Read();
        }

        #endregion
    }
}
