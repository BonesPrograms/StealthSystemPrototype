using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using XRL.Rules;
using XRL.World;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    /// <summary>
    /// Represents the range of values that starts with the assigned <see cref="Start"/> and less than or equal to the <see cref="Length"/>.
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
                Min = Source.Start;
                Max = Source.Length;
                Index = Min - 1;
                Offset = 0;
                Step = 1;
            }
            public Enumerator(InclusiveRange Source, int Offset, int Step = 1)
            {
                InclusiveRange = Source;
                Min = Source.Start;
                Max = Source.Length;
                Index = Min - 1;
                this.Offset = Offset;
                this.Step = Step;
            }

            public bool MoveNext()
            {
                if (Min != InclusiveRange.Start
                    || Max != InclusiveRange.Length)
                    throw new CollectionModifiedException(typeof(InclusiveRange));
                return ++Index <= InclusiveRange.Length;
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
        public int Start;
        [NonSerialized]
        public int Length;

        public int Min
        {
            readonly get => Math.Min(Start, Start + Length);
            set
            {
                if (Min == Start)
                    Start = value;
                else
                    Length += value - Min;
            }
        }
        public int Max
        {
            readonly get => Math.Max(Start, Start + Length);
            set
            {
                if (Max == Start + Length)
                    Length -= value;
                else
                    Length = Min - value;
            }
        }

        public readonly int Direction => Length.Clamp(-1, 1);

        public readonly bool IsForward => Direction >= 0;
        public readonly bool IsBackwards => Direction < 0;

        public InclusiveRange(int Start, int Length)
        {
            this.Start = Start;
            this.Length = Length;
        }
        public InclusiveRange(int Value)
            : this(Math.Min(Value, 0), Math.Max(0, Value))
        {
        }
        public InclusiveRange(InclusiveRange Source)
            : this(Source.Start, Source.Length)
        {
        }
        public InclusiveRange(int Min, InclusiveRange Source)
            : this(Source)
        {
            this.Min = Min;
        }
        public InclusiveRange(InclusiveRange Source, int Max)
            : this(Source)
        {
            this.Max = Max;
        }
        /// <summary>
        /// Constructs a new <see cref="InclusiveRange"/> with <see cref="Start"/> and <see cref="Length"/> values representing the breadth between 0 and the passed <paramref name="Value"/>.
        /// </summary>
        /// <remarks>
        /// A positive <paramref name="Value"/> (eg. 6) will result in a <see cref="Start"/> value of 0 and a <see cref="Length"/> value of <paramref name="Value"/> (6);<br />
        /// A negative <paramref name="Value"/> (eg. -12) will result in a <see cref="Start"/> value of <paramref name="Value"/> (-12) and a <see cref="Length"/> value of 0;
        /// </remarks>
        /// <param name="Value"></param>
        public InclusiveRange(Range Range)
            :this()
        {
            GetValuesFromRange(Range, out Start, out Length);
        }
        public InclusiveRange(int Value, Range Range)
            : this()
        {
            GetValuesFromRange(Range, out Start, out Length);
            Start += Value;
            Length += Value;
        }
        public InclusiveRange(DieRoll DieRoll)
            : this()
        {
            GetValuesFromDieRoll(DieRoll, out Start, out Length);
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

            if (new DieRoll(Value) is var dieRoll)
            {
                GetValuesFromDieRoll(dieRoll, out Start, out Length);
            }
            else
            {
                ArgumentException argEx = new("Failed to parse argument \"" + Value + "\" into valid " + nameof(Start) + " and " + nameof(Length) + " values.", nameof(Value));

                if (!splitString.IsNullOrEmpty())
                {
                    if (Value.Split(splitString) is not string[] values
                        || !int.TryParse(values[0], out Start)
                        || !int.TryParse(values[0], out Length))
                        throw argEx;
                }
                else
                if (int.TryParse(Value, out int value))
                {
                    Start = Math.Min(value, 0);
                    Length = Math.Max(0, value);
                }
                else
                    throw argEx;
            }
        }

        public readonly void Deconstruct(out int Min, out int Max)
        {
            Min = this.Start;
            Max = this.Length;
        }

        private readonly string ExceptionStringBetween(bool Inclusive = false)
            => nameof(Start) + " (" + Start + ") and " + nameof(Length) + " (" + Start + ")" + (Inclusive ? ", inclusive (" + Breadth() + ")." : null);

        public readonly int this[int Index]
            => (GetValues()?.ToArray() ?? new int[0])[Index];

        public readonly int this[Index Index]
            => (GetValues()?.ToArray() ?? new int[0])[Index];

        public readonly IEnumerable<int> this[Range Range]
            => (GetValues()?.ToArray() ?? new int[0])[Range];

        /// <summary>
        /// Gets a <paramref name="Start"/> and <paramref name="Length"/> value from a <see cref="Range"/>, treating <see cref="Range.Start"/> as the starting point (plus <paramref name="StartingValue"/> if one is passed), then using <see cref="Range.End"/> to determine the relative breadth of the resultant <paramref name="Start"/> and <paramref name="Length"/> values. Where a given <see cref="Index.IsFromEnd"/>, its Value will be treated as a negative.
        /// </summary>
        /// <remarks>
        /// A <see cref="Range"/> with ^6 assigned to <see cref="Range.Start"/> and ^12 assigned to <see cref="Range.End"/> will start with -6 for the first value and wants -12 relative to that (-18) for its second value. <paramref name="Start"/> must always be less than <paramref name="Length"/>, so <paramref name="Start"/> is assigned -18, and <paramref name="Length"/> is assigned -6.
        /// </remarks>
        /// <param name="Range">The <see cref="Range"/> from which <paramref name="Start"/> and <paramref name="Length"/> value will be derived.</param>
        /// <param name="Start">An <see cref="int"/> representation of <see cref="Range.Start"/>. When <see cref="Range.Start.IsFromEnd"/>, this value will be negative.</param>
        /// <param name="Length">The difference between <param name="Start"> and an <see cref="int"/> representation of <see cref="Range.End"/>. When <see cref="Range.End.IsFromEnd"/>, <see cref="Range.End.Value"/> will be treated as a negative.</param>
        public static void GetValuesFromRange(Range Range, out int Start, out int Length)
        {
            GetMinMax(out int min, out int max,
                new int[]
                {
                    Range.Start.GetIntValue(),
                    Range.End.GetIntValue()
                });

            Start = min;
            Length = max - Start;
        }
        /// <summary>
        /// Gets an <see cref="InclusiveRange"/> with <see cref="Start"/> and <see cref="Length"/> values derived from a passed <see cref="Range"/> and optional <paramref name="StartingValue"/>, treating <see cref="Range.Start"/> as the starting point (plus <paramref name="StartingValue"/> if one is passed), then using <see cref="Range.End"/> to determine the relative breadth of the resultant <see cref="InclusiveRange"/>. Where a given <see cref="Index.IsFromEnd"/>, its Value will be treated as a negative.
        /// </summary>
        /// <remarks>
        /// See <seealso cref="GetValuesFromRange"/> for implementation of method that derives the mentioned values.
        /// </remarks>
        /// <param name="Range">The <see cref="Range"/> from which <see cref="Start"/> and <see cref="Length"/> value will be derived.</param>
        /// <param name="StartingValue">An optional amount to add to <paramref name="Range"/>'s <see cref="Range.Start"/> value before deriving the breadth of the values.</param>
        /// <returns>An <see cref="InclusiveRange"/> with <see cref="Start"/> and <see cref="Length"/> values derived from the passed <see cref="Range"/></returns>
        public static InclusiveRange GetFromRange(Range Range)
        {
            GetValuesFromRange(Range, out int min, out int max);
            return new(min, max);
        }

        public static void GetValuesFromDieRoll(DieRoll DieRoll, out int Start, out int Length)
        {
            if (DieRoll.IsConstantOnlyRangeType())
            {
                Start = DieRoll.LeftValue;
                Length = DieRoll.RightValue - Start;
            }
            else
            {
                Start = DieRoll.Min();
                Length = DieRoll.Max() - Start;
            }
        }
        public static InclusiveRange GetFromDieRoll(DieRoll DieRoll)
        {
            GetValuesFromDieRoll(DieRoll, out int start, out int length);
            return new InclusiveRange(start, length);
        }

        public readonly string ToString(bool Full)
            => Min + "_" + Max + (Full ? ":" + Start + "," + Length : null);

        public override readonly string ToString()
            => ToString(true);

        public readonly bool Contains(int Value)
            => Min <= Value
            && Value <= Max;

        public readonly InclusiveRange Clamp(int Min, int Max)
        {
            int min = this.Min.Clamp(Min, Max);
            int max = this.Max.Clamp(Min, Max);
            int start;
            int length;
            if (IsForward)
            {
                start = min;
                length = max - start;
            }
            else
            {
                start = max;
                length = min - start;
            }
            return new(start, length);
        }

        public readonly InclusiveRange Clamp(InclusiveRange OtherRange)
            => Clamp(OtherRange.Start, OtherRange.Length);

        public readonly InclusiveRange AdjustBy(int Value)
            => new(Start + Value, Length + Value);

        public readonly InclusiveRange AdjustBy(InclusiveRange OtherRange)
            => new(Start + OtherRange.Start, Length + OtherRange.Length);

        public readonly InclusiveRange AdjustByClamped(int Value, InclusiveRange Clamp)
            => AdjustBy(Value).Clamp(Clamp);

        public readonly InclusiveRange AdjustByClamped(InclusiveRange OtherRange, InclusiveRange Clamp)
            => AdjustBy(OtherRange).Clamp(Clamp);

        public readonly IEnumerable<int> GetValues(int Offset = 0, int Step = 1)
        {
            if (Equals(Empty)
                || Equals(default))
                yield break;

            for (int i = Offset + Start; i < Offset + Length; i++)
                yield return i * Step;
        }

        public readonly Range ToRange()
            => new Index(Min, Min < 0)..new Index(Max, Max < 0);

        public readonly DieRoll ToDieRoll()
            => new(DieRoll.TYPE_RANGE, Min, Max);

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

            return Start.Equals(Other.Start)
                && Length.Equals(Other.Start);
        }
        public override readonly int GetHashCode()
            => Start.GetHashCode()
            ^ Length.GetHashCode();

        public readonly int Sum(bool IncludeIntermediateValues = false)
            => !IncludeIntermediateValues
            ? Start + Length
            : this.Aggregate(0, (a, n) => a + n);

        public readonly int Average(bool IncludeIntermediateValues = false)
            => !IncludeIntermediateValues
            ? Utils.Average(Start, Length)
            : Utils.Average((int[])this);

        public readonly int Breadth()
            => Length - Start;

        public readonly int CompareSum(InclusiveRange Other, bool IncludeIntermediateValues = false)
            => Sum(IncludeIntermediateValues) - Other.Sum(IncludeIntermediateValues);

        public readonly int CompareAverage(InclusiveRange Other, bool IncludeIntermediateValues = false)
            => Average(IncludeIntermediateValues) - Other.Average(IncludeIntermediateValues);

        public readonly int CompareBreadth(InclusiveRange Other)
            => Breadth() - Other.Breadth();

        public readonly int CompareMin(InclusiveRange Other)
            => Start - Other.Start;

        public readonly int CompareMax(InclusiveRange Other)
            => Start - Other.Start;

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
            Writer.WriteOptimized(Start);
            Writer.WriteOptimized(Length);
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
            ReadOptimizedInclusiveRange(Reader, out Start, out Length);
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
