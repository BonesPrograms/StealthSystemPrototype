using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class ClampedBreadth : IComposite
    {
        public static ClampedBreadth Empty => new(StealthSystemPrototype.Breadth.Empty, StealthSystemPrototype.Breadth.Empty);

        private Breadth Value;
        private Breadth Clamp;

        private ClampedBreadth()
        {
            Value = default;
            Clamp = default;
        }

        public ClampedBreadth(Breadth Value, Breadth Clamp)
        {
            this.Value = Value.Clamp(Clamp);
            this.Clamp = Clamp;
        }
        public ClampedBreadth(Breadth Value)
            : this(Value, Value)
        {
        }
        public ClampedBreadth(ClampedBreadth Value, Breadth Clamp)
            : this(Value.Value, Clamp)
        {
        }
        public ClampedBreadth(Breadth Value, ClampedBreadth Clamp)
            : this(Value, Clamp.Clamp)
        {
        }

        public void Deconstruct(out Breadth Range)
        {
            Range = GetBreadth();
        }

        public override string ToString()
            => GetBreadth().ToString();

        public static Breadth GetBreadth(Breadth Breadth, Breadth Clamp)
            => Breadth.Clamp(Clamp);

        public Breadth GetBreadth()
            => GetBreadth(Value, Clamp);

        public ClampedBreadth SetValue(Breadth Value)
            => new(Value, Clamp);

        public ClampedBreadth SetClamp(Breadth Clamp)
            => new(Value, Clamp);

        public ClampedBreadth AdjustClamp(Breadth Clamp)
            => new(Value, Clamp);

        public ClampedBreadth AdjustBy(int Amount)
            => new(Value.AdjustBy(Amount).Clamp(Clamp), this);

        public ClampedBreadth AdjustBy(Breadth OtherBreadth)
            => new(Value.AdjustBy(OtherBreadth), this);

        public ClampedBreadth AdjustClampBy(int Amount)
            => new(this, Value.AdjustBy(Amount).Clamp(Clamp));

        public ClampedBreadth AdjustClampBy(Breadth OtherBreadth)
            => new(this, Clamp.AdjustBy(OtherBreadth));

        public int Sum()
            => GetBreadth().Sum();

        public int Average()
            => GetBreadth().Average();

        public int Difference()
            => GetBreadth().Difference();

        public int Floor()
            => GetBreadth().Min;

        public int Ceiling()
            => GetBreadth().Max;

        public DieRoll GetDieRoll()
            => new(DieRoll.TYPE_RANGE, GetBreadth().Min, GetBreadth().Max);

        public int Roll(bool Cosmetic = true)
            => Cosmetic 
            ? RandomCosmetic()
            : Random();

        public int Random()
            => GetBreadth().Random();

        public int RandomCosmetic()
            => GetBreadth().RandomCosmetic();

        public int SeededRandom(string Seed)
            => GetBreadth().SeededRandom(Seed);

        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            Breadth Value,
            Breadth Clamp)
        {
            Value.WriteOptimized(Writer);
            Clamp.WriteOptimized(Writer);
        }

        public static void WriteOptimized(
            SerializationWriter Writer,
            ClampedBreadth ClampedRange)
            => WriteOptimized(Writer, ClampedRange.Value, ClampedRange.Clamp);

        public static void ReadOptimizedClampedRange(
            SerializationReader Reader,
            out Breadth Value,
            out Breadth Clamp)
        {
            Value = Breadth.ReadOptimizedBreadth(Reader);
            Clamp = Breadth.ReadOptimizedBreadth(Reader);
        }
        public static ClampedBreadth ReadOptimizedClampedRange(SerializationReader Reader)
        {
            ReadOptimizedClampedRange(Reader, out Breadth value, out Breadth clamp);
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

        public static implicit operator Breadth(ClampedBreadth Operand)
            => Operand.GetBreadth();

        public static explicit operator ClampedBreadth(Breadth Operand)
            => new(Operand);

        #endregion
    }
}
