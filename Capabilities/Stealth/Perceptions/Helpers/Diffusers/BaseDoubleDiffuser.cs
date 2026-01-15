using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public abstract class BaseDoubleDiffuser : SerializableSequence<double>
    {
        public double Amount;

        #region Constructors

        public BaseDoubleDiffuser()
            : base()
        {
        }
        public BaseDoubleDiffuser(InclusiveRange Steps, double StartValue)
            : base(Steps, StartValue)
        {
        }
        public BaseDoubleDiffuser(double Amount, InclusiveRange Steps, double StartValue)
            : this(Steps, StartValue)
        {
            this.Amount = Amount;
        }
        public BaseDoubleDiffuser(double Amount, InclusiveRange Steps)
            : this(Amount, Steps, 1.0)
        {
        }

        #endregion

        public override string ToString(bool Short, string FormatString = null)
        {
            return base.ToString(Short, FormatString ?? WithDigitsFormat(2));
        }

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

            Amount = Reader.ReadDouble();
        }

        #endregion
    }
}
