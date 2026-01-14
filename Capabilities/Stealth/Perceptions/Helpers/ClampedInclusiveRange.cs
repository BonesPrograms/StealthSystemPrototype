using System;
using System.Collections.Generic;
using System.Text;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class ClampedInclusiveRange : IComposite
    {
        public static ClampedInclusiveRange Empty => new(InclusiveRange.Empty, InclusiveRange.Empty);

        private InclusiveRange Value;
        private InclusiveRange Clamp;

        private ClampedInclusiveRange()
        {
            Value = default;
            Clamp = default;
        }
        public ClampedInclusiveRange(InclusiveRange Value, InclusiveRange Clamp)
        {
            this.Value = Value.Clamp(Clamp);
            this.Clamp = Clamp;
        }
        public ClampedInclusiveRange(InclusiveRange Value)
            : this(Value, Value)
        {
        }
        public ClampedInclusiveRange(ClampedInclusiveRange Value, InclusiveRange Clamp)
            : this(Value.Value, Clamp)
        {
        }
        public ClampedInclusiveRange(InclusiveRange Value, ClampedInclusiveRange Clamp)
            : this(Value, Clamp.Clamp)
        {
        }
        public ClampedInclusiveRange(Range Range, Range Clamp)
            : this(new InclusiveRange(Range), new InclusiveRange(Clamp))
        {
        }
        public ClampedInclusiveRange(int Value, Range Clamp)
            : this(new InclusiveRange(Value), new InclusiveRange(Value, Clamp))
        {
        }

        public void Deconstruct(out InclusiveRange InclusiveRange)
        {
            InclusiveRange = GetInclusiveRange();
        }

        public override string ToString()
            => GetInclusiveRange().ToString();

        public static InclusiveRange GetInclusiveRange(InclusiveRange Breadth, InclusiveRange Clamp)
            => Breadth.Clamp(Clamp);

        public InclusiveRange GetInclusiveRange()
            => GetInclusiveRange(Value, Clamp);

        public ClampedInclusiveRange SetValue(InclusiveRange Value)
            => new(Value, Clamp);

        public ClampedInclusiveRange SetClamp(InclusiveRange Clamp)
            => new(Value, Clamp);

        public ClampedInclusiveRange AdjustClamp(InclusiveRange Clamp)
            => new(Value, Clamp);

        public ClampedInclusiveRange AdjustBy(int Amount)
            => new(Value.AdjustBy(Amount).Clamp(Clamp), this);

        public ClampedInclusiveRange AdjustBy(InclusiveRange OtherBreadth)
            => new(Value.AdjustBy(OtherBreadth), this);

        public ClampedInclusiveRange AdjustClampBy(int Amount)
            => new(this, Value.AdjustBy(Amount).Clamp(Clamp));

        public ClampedInclusiveRange AdjustClampBy(InclusiveRange OtherBreadth)
            => new(this, Clamp.AdjustBy(OtherBreadth));

        public int Sum()
            => GetInclusiveRange().Sum();

        public int Average()
            => GetInclusiveRange().Average();

        public int Difference()
            => GetInclusiveRange().Breadth();

        public int Floor()
            => GetInclusiveRange().Min;

        public int Ceiling()
            => GetInclusiveRange().Max;

        public DieRoll GetDieRoll()
            => new(DieRoll.TYPE_RANGE, GetInclusiveRange().Min, GetInclusiveRange().Max);

        public int Roll(bool Cosmetic = true)
            => Cosmetic 
            ? RandomCosmetic()
            : Random();

        public int Random()
            => GetInclusiveRange().Random();

        public int RandomCosmetic()
            => GetInclusiveRange().RandomCosmetic();

        public int SeededRandom(string Seed)
            => GetInclusiveRange().SeededRandom(Seed);

        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            InclusiveRange Value,
            InclusiveRange Clamp)
        {
            Value.WriteOptimized(Writer);
            Clamp.WriteOptimized(Writer);
        }

        public static void WriteOptimized(
            SerializationWriter Writer,
            ClampedInclusiveRange ClampedRange)
            => WriteOptimized(Writer, ClampedRange.Value, ClampedRange.Clamp);

        public static void ReadOptimizedClampedRange(
            SerializationReader Reader,
            out InclusiveRange Value,
            out InclusiveRange Clamp)
        {
            Value = InclusiveRange.ReadOptimizedInclusiveRange(Reader);
            Clamp = InclusiveRange.ReadOptimizedInclusiveRange(Reader);
        }
        public static ClampedInclusiveRange ReadOptimizedClampedRange(SerializationReader Reader)
        {
            ReadOptimizedClampedRange(Reader, out InclusiveRange value, out InclusiveRange clamp);
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

        public static implicit operator InclusiveRange(ClampedInclusiveRange Operand)
            => Operand.GetInclusiveRange();

        public static explicit operator ClampedInclusiveRange(InclusiveRange Operand)
            => new(Operand);

        #endregion
    }
}
