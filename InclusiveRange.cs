using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using XRL.World;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    /// <summary>
    /// Represents an inclusive range of values that are greater than or equal to the assigned <see cref="Min"/> and less than or equal to the <see cref="Max"/>.
    /// </summary>
    [Serializable]
    public struct InclusiveRange : IComposite, IEquatable<InclusiveRange>, IComparable<InclusiveRange>, IEnumerable<int>
    {
        [Serializable]
        public struct Enumerator
            : IEnumerator<int>
            , IEnumerator
            , IDisposable
        {
            private InclusiveRange InclusiveRange;
            private int Min;
            private int Max;
            private int Index;
            private int Offset;
            private int Step;

            public readonly int Current => (Index + Offset) * Step;
            readonly object IEnumerator.Current => Current;

            public Enumerator(InclusiveRange Source)
            {
                InclusiveRange = Source;
                Min = Source.Min;
                Max = Source.Max;
                Index = Min - 1;
                Offset = 0;
                Step = 1;
            }
            public Enumerator(InclusiveRange Source, int Offset, int Step = 1)
            {
                InclusiveRange = Source;
                Min = Source.Min;
                Max = Source.Max;
                Index = Min - 1;
                this.Offset = Offset;
                this.Step = Step;
            }

            public bool MoveNext()
            {
                if (Min != InclusiveRange.Min
                    || Max != InclusiveRange.Max)
                    throw new CollectionModifiedException(typeof(InclusiveRange));
                return ++Index <= InclusiveRange.Max;
            }

            public void Reset()
            {
                Index = Min - 1;
            }

            public void Dispose()
            {
                InclusiveRange = default;
                Min = default;
                Max = default;
                Index = default;
                Offset = default;
                Step = default;
            }
        }

        public static InclusiveRange Empty => new(0, 0);

        [NonSerialized]
        public int Min;
        [NonSerialized]
        public int Max;

        public InclusiveRange(int Min, int Max)
        {
            this.Min = Min;
            this.Max = Max;
            CheckThrowArgumentOutOfRangeException();
        }
        /// <summary>
        /// Constructs a new <see cref="InclusiveRange"/> with <see cref="Min"/> and <see cref="Max"/> values representing the breadth between 0 and the passed <paramref name="Value"/>.
        /// </summary>
        /// <remarks>
        /// A positive <paramref name="Value"/> (eg. 6) will result in a <see cref="Min"/> value of 0 and a <see cref="Max"/> value of <paramref name="Value"/> (6);<br />
        /// A negative <paramref name="Value"/> (eg. -12) will result in a <see cref="Min"/> value of <paramref name="Value"/> (-12) and a <see cref="Max"/> value of 0;
        /// </remarks>
        /// <param name="Value"></param>
        public InclusiveRange(int Value)
            : this(Math.Min(Value, 0), Math.Max(0, Value))
        {
        }
        public InclusiveRange(InclusiveRange Source)
            : this(Source.Min, Source.Max)
        {
        }
        public InclusiveRange(int Min, InclusiveRange Source)
            : this(Min, Source.Max)
        {
        }
        public InclusiveRange(InclusiveRange Source, int Max)
            : this(Source.Min, Max)
        {
        }
        public InclusiveRange(Range Range)
            :this()
        {
            GetValuesFromRange(Range, out Min, out Max);
            CheckThrowArgumentOutOfRangeException();
        }
        public InclusiveRange(int Value, Range Range)
            : this()
        {
            GetValuesFromRange(Range, out Min, out Max, Value);
            CheckThrowArgumentOutOfRangeException();
        }
        public InclusiveRange(string Value)
            : this()
        {
            string splitString = null;

            if (Value.Contains("-"))
                splitString = "-";
            else
            if (Value.Contains(".."))
                splitString = "..";

            ArgumentException argEx = new("Failed to parse argument \"" + Value + "\" into valid " + nameof(Min) + " and " + nameof(Max) + " values.", nameof(Value));

            if (!splitString.IsNullOrEmpty())
            {
                if (Value.Split(splitString) is not string[] values
                    || !int.TryParse(values[0], out Min)
                    || !int.TryParse(values[0], out Max))
                    throw argEx;
            }
            else
            if (int.TryParse(Value, out int value))
            {
                Min = Math.Min(value, 0);
                Max = Math.Max(0, value);
            }
            else
                throw argEx;
        }

        private readonly void CheckThrowArgumentOutOfRangeException()
        {
            if (Min > Max)
                throw new ArgumentOutOfRangeException(nameof(Min), "cannot be greater than " + nameof(Max) + ".");
        }

        public readonly void Deconstruct(out int Min, out int Max)
        {
            Min = this.Min;
            Max = this.Max;
        }

        private readonly string ExceptionStringBetween(bool Inclusive = false)
            => nameof(Min) + " (" + Min + ") and " + nameof(Max) + " (" + Min + ")" + (Inclusive ? ", inclusive (" + Breadth() + ")." : null);

        public readonly int this[int Index]
        {
            get
            {
                if (Index < 0
                    || Index > Breadth())
                    throw new ArgumentOutOfRangeException(nameof(Index), "must be between 0 and the difference between " + ExceptionStringBetween(Inclusive: true));
                return Min + Index;
            }
        }

        public readonly int this[Index Index]
        {
            get
            {
                if (Index.Value < 0
                    || Index.Value > Breadth())
                    throw new ArgumentOutOfRangeException(nameof(Index), "must be between 0 and the difference between " + ExceptionStringBetween(Inclusive: true));

                return !Index.IsFromEnd
                    ? Min + Index.Value
                    : Max - Index.Value;
            }
        }

        public readonly IEnumerable<int> this[Range Range]
        {
            get
            {
                if (Range.Start.Value > Breadth()
                    || Range.End.Value > Breadth())
                    throw new ArgumentOutOfRangeException(nameof(Range), "must be between 0 and the difference between " + ExceptionStringBetween(Inclusive: true));
                int start = this[Range.Start];
                int end = this[Range.End];
                for (int i = 0; i < end - start; i++)
                    yield return i + start;
            }
        }

        /// <summary>
        /// Gets a <paramref name="Min"/> and <paramref name="Max"/> from a <see cref="Range"/> and optional <paramref name="StartingValue"/>, treating <see cref="Range.Start"/> as the starting point (plus <paramref name="StartingValue"/> if one is passed), then using <see cref="Range.End"/> to determine the relative breadth of the resultant <paramref name="Min"/> and <paramref name="Max"/> values. Where a given <see cref="Index.IsFromEnd"/>, its Value will be treated as a negative.
        /// </summary>
        /// <remarks>
        /// A <see cref="Range"/> with ^6 assigned to <see cref="Range.Start"/> and ^12 assigned to <see cref="Range.End"/> will start with -6 for the first value and wants -12 relative to that (-18) for its second value. <paramref name="Min"/> must always be less than <paramref name="Max"/>, so <paramref name="Min"/> is assigned -18, and <paramref name="Max"/> is assigned -6.
        /// </remarks>
        /// <param name="Range">The <see cref="Range"/> from which <paramref name="Min"/> and <paramref name="Max"/> value will be derived.</param>
        /// <param name="Min">The smaller of the two derived values.</param>
        /// <param name="Max">The larger of the two derived values.</param>
        /// <param name="StartingValue">An optional amount to add to <paramref name="Range"/>'s <see cref="Range.Start"/> value before deriving the breadth of the values.</param>
        public static void GetValuesFromRange(Range Range, out int Min, out int Max, int StartingValue = 0)
        {
            int startPoint = StartingValue + Range.Start.GetIntValue();
            int endPoint = startPoint + Range.End.GetIntValue();
            Min = Math.Min(startPoint, endPoint);
            Max = Math.Max(startPoint, endPoint);
        }
        /// <summary>
        /// Gets an <see cref="InclusiveRange"/> with <see cref="Min"/> and <see cref="Max"/> values derived from a passed <see cref="Range"/> and optional <paramref name="StartingValue"/>, treating <see cref="Range.Start"/> as the starting point (plus <paramref name="StartingValue"/> if one is passed), then using <see cref="Range.End"/> to determine the relative breadth of the resultant <see cref="InclusiveRange"/>. Where a given <see cref="Index.IsFromEnd"/>, its Value will be treated as a negative.
        /// </summary>
        /// <remarks>
        /// See <seealso cref="GetValuesFromRange"/> for implementation of method that derives the mentioned values.
        /// </remarks>
        /// <param name="Range">The <see cref="Range"/> from which <see cref="Min"/> and <see cref="Max"/> value will be derived.</param>
        /// <param name="StartingValue">An optional amount to add to <paramref name="Range"/>'s <see cref="Range.Start"/> value before deriving the breadth of the values.</param>
        /// <returns>An <see cref="InclusiveRange"/> with <see cref="Min"/> and <see cref="Max"/> values derived from the passed <see cref="Range"/></returns>
        public static InclusiveRange GetFromRange(Range Range, int StartingValue = 0)
        {
            GetValuesFromRange(Range, out int min, out int max, StartingValue);
            return new(min, max);
        }

        public override readonly string ToString()
            => Min + ".." + Max;

        public readonly bool Contains(int Value)
            => Min <= Value
            && Value <= Max;

        public readonly InclusiveRange Clamp(int Min, int Max)
            => new(this.Min.Clamp(Min, Max), this.Max.Clamp(Min, Max));

        public readonly InclusiveRange Clamp(InclusiveRange Breadth)
            => Clamp(Breadth.Min, Breadth.Max);

        public readonly InclusiveRange AdjustBy(int Value)
            => new(Min + Value, Max + Value);

        public readonly InclusiveRange AdjustBy(InclusiveRange OtherRange)
            => new(Min + OtherRange.Min, Max + OtherRange.Max);

        public readonly InclusiveRange AdjustByClamped(int Value, InclusiveRange Clamp)
            => AdjustBy(Value).Clamp(Clamp);

        public readonly InclusiveRange AdjustByClamped(InclusiveRange OtherRange, InclusiveRange Clamp)
            => AdjustBy(OtherRange).Clamp(Clamp);

        public readonly IEnumerable<int> GetValues(int Offset = 0, int Step = 1)
        {
            if (Equals(Empty)
                || Equals(default))
                yield break;

            for (int i = Offset + Min; i < Offset + Max; i++)
                yield return i * Step;
        }

        public readonly Range ToRange()
            => 0..Breadth();

        public readonly int[] ToArray()
            => GetValues()?.ToArray() ?? new int[0];

        public override readonly bool Equals(object obj)
        {
            if (obj is InclusiveRange breadthObj)
                Equals(breadthObj);

            return base.Equals(obj);
        }
        public readonly bool Equals(InclusiveRange Other)
        {
            if (EitherNull(this, Other, out bool areEqual))
                return areEqual;

            return Min.Equals(Other.Min)
                && Max.Equals(Other.Min);
        }
        public override readonly int GetHashCode()
            => Min.GetHashCode()
            ^ Max.GetHashCode();

        public readonly int Sum(bool IncludeIntermediateValues = false)
            => !IncludeIntermediateValues
            ? Min + Max
            : this.Aggregate(0, (a, n) => a + n);

        public readonly int Average(bool IncludeIntermediateValues = false)
            => !IncludeIntermediateValues
            ? Utils.Average(Min, Max)
            : Utils.Average((int[])this);

        public readonly int Breadth()
            => Max - Min;

        public readonly int CompareSum(InclusiveRange Other, bool IncludeIntermediateValues = false)
            => Sum(IncludeIntermediateValues) - Other.Sum(IncludeIntermediateValues);

        public readonly int CompareAverage(InclusiveRange Other, bool IncludeIntermediateValues = false)
            => Average(IncludeIntermediateValues) - Other.Average(IncludeIntermediateValues);

        public readonly int CompareBreadth(InclusiveRange Other)
            => Breadth() - Other.Breadth();

        public readonly int CompareMin(InclusiveRange Other)
            => Min - Other.Min;

        public readonly int CompareMax(InclusiveRange Other)
            => Min - Other.Min;

        public readonly int CompareMinThenMax(InclusiveRange Other)
            => CompareMin(Other) is int floorComp
                && floorComp == 0
            ? CompareMax(Other)
            : floorComp;

        public readonly int CompareMaxThenMin(InclusiveRange Other)
            => CompareMax(Other) is int ceilingComp
                && ceilingComp == 0
            ? CompareMin(Other)
            : ceilingComp;

        public readonly int CompareTo(InclusiveRange Other)
            => CompareSum(Other)
            + CompareAverage(Other)
            + CompareBreadth(Other)
            + CompareMin(Other)
            + CompareMin(Other);

        public readonly IEnumerator<int> GetEnumerator()
            => new Enumerator(this);

        readonly IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();

        public readonly void WriteOptimized(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Min);
            Writer.WriteOptimized(Max);
        }

        public static void ReadOptimizedInclusiveRange(SerializationReader Reader, out int Min, out int Max)
        {
            Min = Reader.ReadOptimizedInt32();
            Max = Reader.ReadOptimizedInt32();
        }
        public static InclusiveRange ReadOptimizedInclusiveRange(SerializationReader Reader)
        {
            ReadOptimizedInclusiveRange(Reader, out int min, out int max);
            return new(min, max);
        }

        public readonly void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedInclusiveRange(Reader, out Min, out Max);
        }

        public static bool operator ==(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.Equals(Operand2);

        public static bool operator !=(InclusiveRange Operand1, InclusiveRange Operand2)
            => !(Operand1 == Operand2);

        public static bool operator >(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) > 0;

        public static bool operator <(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) < 0;

        public static bool operator >=(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) >= 0;

        public static bool operator <=(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) <= 0;

        public static explicit operator int[](InclusiveRange InclusiveRange)
            => InclusiveRange.ToArray();

        public static implicit operator InclusiveRange(Range Range)
            => new(Range);
    }
}
