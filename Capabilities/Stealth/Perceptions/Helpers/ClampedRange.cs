using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public struct ClampedRange : IComposite
    {
        public static ClampedRange Empty => new(0..0, 0..0);

        private Range Value;
        private Range Clamp;

        public ClampedRange(Range Value, Range Clamp)
        {
            this.Value = Value;
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

        public readonly void Deconstruct(out Range Range)
        {
            Range = this.GetRange();
        }

        public static Range GetRange(Range Range, Range Clamp)
            => Range.ClampRange(Clamp);

        public readonly Range GetRange()
            => GetRange(Value, Clamp);

        public readonly ClampedRange SetValue(Range Value)
            => new(Value, Clamp);

        public readonly ClampedRange SetClamp(Range Clamp)
            => new(Value, Clamp);

        public readonly ClampedRange AdjustClamp(Range Clamp)
            => new(Value, Clamp);

        public readonly ClampedRange AdjustBy(int Amount)
            => new(Value.AdjustBy(Amount).ClampRange(Clamp), this);

        public readonly ClampedRange AdjustBy(Range OtherRange)
            => new(Value.AdjustBy(OtherRange), this);

        public readonly ClampedRange AdjustClampBy(int Amount)
            => new(this, Value.AdjustBy(Amount).ClampRange(Clamp));

        public readonly ClampedRange AdjustClampBy(Range OtherRange)
            => new(this, Clamp.AdjustBy(OtherRange));

        public readonly int Sum()
            => GetRange().Sum();

        public readonly int Average()
            => GetRange().Average();

        public readonly int Breadth()
            => GetRange().Breadth();

        public readonly int Floor()
            => GetRange().Floor();

        public readonly int Ceiling()
            => GetRange().Ceiling();

        public readonly string GetDieRollString()
            => "1d" + (GetRange().End.Value - GetRange().Start.Value) + "+" + GetRange().Start.Value;

        public readonly string GetDieRollRangeString()
            => GetRange().Start.Value + "-" + GetRange().End.Value;

        public readonly DieRoll GetDieRoll()
            => new(DieRoll.TYPE_RANGE, GetRange().Start.Value, GetRange().End.Value);

        public readonly int Roll()
            => Stat.Roll(GetRange().Start.Value, GetRange().End.Value);

        public readonly int Random()
            => Stat.Random(GetRange().Start.Value, GetRange().End.Value);

        public readonly int RandomCosmetic()
            => Stat.RandomCosmetic(GetRange().Start.Value, GetRange().End.Value);

        public readonly int SeededRandom(string Seed)
            => Stat.SeededRandom(Seed, GetRange().Start.Value, GetRange().End.Value);

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

        public readonly void Write(SerializationWriter Writer)
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
