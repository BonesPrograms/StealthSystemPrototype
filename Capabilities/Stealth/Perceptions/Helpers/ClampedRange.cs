using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class ClampedRange : IComposite
    {
        public static ClampedRange Empty => new(0..0, 0..0);

        private Range Value;
        private Range Clamp;

        private ClampedRange()
        {
            Value = default;
            Clamp = default;
        }

        public ClampedRange(Range Value, Range Clamp)
        {
            this.Value = Value.ClampRange(Clamp);
            this.Clamp = Clamp;
        }
        public ClampedRange(Range Value)
            : this(Value, Value)
        {
        }
        public ClampedRange(ClampedRange Value, Range Clamp)
            : this(Value.Value, Clamp)
        {
        }
        public ClampedRange(Range Value, ClampedRange Clamp)
            : this(Value, Clamp.Clamp)
        {
        }

        public void Deconstruct(out Range Range)
        {
            Range = GetRange();
        }

        public override string ToString()
            => GetRange().ToString();

        public static Range GetRange(Range Range, Range Clamp)
            => Range.ClampRange(Clamp);

        public Range GetRange()
            => GetRange(Value, Clamp);

        public ClampedRange SetValue(Range Value)
            => new(Value, Clamp);

        public ClampedRange SetClamp(Range Clamp)
            => new(Value, Clamp);

        public ClampedRange AdjustClamp(Range Clamp)
            => new(Value, Clamp);

        public ClampedRange AdjustBy(int Amount)
            => new(Value.AdjustBy(Amount).ClampRange(Clamp), this);

        public ClampedRange AdjustBy(Range OtherRange)
            => new(Value.AdjustBy(OtherRange), this);

        public ClampedRange AdjustClampBy(int Amount)
            => new(this, Value.AdjustBy(Amount).ClampRange(Clamp));

        public ClampedRange AdjustClampBy(Range OtherRange)
            => new(this, Clamp.AdjustBy(OtherRange));

        public int Sum()
            => GetRange().Sum();

        public int Average()
            => GetRange().Average();

        public int Breadth()
            => GetRange().Breadth();

        public int Floor()
            => GetRange().Floor();

        public int Ceiling()
            => GetRange().Ceiling();

        public string GetDieRollString()
            => "1d" + (GetRange().End.Value - GetRange().Start.Value) + "+" + GetRange().Start.Value;

        public string GetDieRollRangeString()
            => GetRange().Start.Value + "-" + GetRange().End.Value;

        public DieRoll GetDieRoll()
            => new(DieRoll.TYPE_RANGE, GetRange().Start.Value, GetRange().End.Value);

        public int Roll(bool Cosmetic = true)
            => Cosmetic 
            ? RandomCosmetic()
            : Random();

        public int Random()
            => GetRange().Random();

        public int RandomCosmetic()
            => GetRange().RandomCosmetic();

        public int SeededRandom(string Seed)
            => GetRange().SeededRandom(Seed);

        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            Range Value,
            Range Clamp)
        {
            Writer.WriteOptimized(Value);
            Writer.WriteOptimized(Clamp);
        }

        public static void WriteOptimized(
            SerializationWriter Writer,
            ClampedRange ClampedRange)
            => WriteOptimized(Writer, ClampedRange.Value, ClampedRange.Clamp);

        public static void ReadOptimizedClampedRange(
            SerializationReader Reader,
            out Range Value,
            out Range Clamp)
        {
            Value = Reader.ReadOptimizedRange();
            Clamp = Reader.ReadOptimizedRange();
        }
        public static ClampedRange ReadOptimizedClampedRange(SerializationReader Reader)
        {
            ReadOptimizedClampedRange(Reader, out Range value, out Range clamp);
            return new(value, clamp);
        }

        public void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer, Value, Clamp);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedClampedRange(Reader, out Value, out Clamp);
        }

        #endregion
        #region Conversions

        public static implicit operator Range(ClampedRange Operand)
            => Operand.GetRange();

        public static explicit operator ClampedRange(Range Operand)
            => new(Operand);

        #endregion
    }
}
