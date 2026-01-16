using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

using StealthSystemPrototype.Logging;

using XRL.Rules;
using XRL.World;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    /// <summary>
    /// Represents the range of values that starts with the assigned <see cref="Start"/> and less than or equal to the <see cref="End"/>.
    /// </summary>
    [Serializable]
    public struct InclusiveRange : IComposite, IEquatable<InclusiveRange>, IComparable<InclusiveRange>, IEnumerable<int>, IDisposable
    {
        [Serializable]
        public struct Enumerator
            : IEnumerator<int>
            , IEnumerator
            , IDisposable
        {
            private InclusiveRange InclusiveRange;
            private int Start;
            private int End;
            private int AbsLength;
            private int Direction;
            private int Index;
            private int Offset;
            private int Step;

            public readonly int Current => (Offset + Start + Index) * Step * Direction;
            readonly object IEnumerator.Current => Current;

            public Enumerator(InclusiveRange Source)
            {
                using Indent indent = new(1);
                Debug.LogMethod(indent,
                    ArgPairs: new Debug.ArgPair[]
                    {
                        Debug.Arg(nameof(Source), Source),
                    });

                InclusiveRange = Source;
                Start = Source.Start;
                End = Source.End;
                AbsLength = Source.AbsLength;
                Direction = Source.Direction;
                Index = -1;
                Offset = 0;
                Step = 1;
            }
            public Enumerator(InclusiveRange Source, int Offset, int Step = 1)
                : this(Source)
            {
                this.Offset = Offset;
                this.Step = Step;
            }

            public bool MoveNext()
            {
                if (Start != InclusiveRange.Start
                    || End != InclusiveRange.End)
                    throw new CollectionModifiedException(typeof(InclusiveRange));
                return ++Index < AbsLength;
            }

            public void Reset()
            {
                Index = -1;
            }

            public void Dispose()
            {
                InclusiveRange = default;
                Start = default;
                End = default;
                AbsLength = default;
                Direction = default;
                Index = default;
                Offset = default;
                Step = default;
            }
        }

        public static InclusiveRange Empty => new(0, 0);

        [NonSerialized]
        public int Start;
        [NonSerialized]
        public int End;

        public readonly int Length => End - Start + Start.CompareTo(End);
        public readonly int AbsLength => Math.Abs(Length);

        public int Min
        {
            readonly get => Math.Min(Start, Start + End);
            set
            {
                if (IsForward)
                    Start = value;
                else
                    End = value;
            }
        }
        public int Max
        {
            readonly get => Math.Max(Start, Start + End);
            set
            {
                if (IsForward)
                    End = value;
                else
                    Start = value;
            }
        }

        public readonly int Direction => Length.Clamp(-1, 1);

        public readonly bool IsForward => Direction >= 0;
        public readonly bool IsBackwards => Direction < 0;

        public InclusiveRange(int Start, int End)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Start), Start),
                    Debug.Arg(nameof(End), End),
                });

            this.Start = Start;
            this.End = End;
        }
        public InclusiveRange(int Value)
            : this(0, Value)
        {
        }
        public InclusiveRange(InclusiveRange Source)
            : this(Source.Start, Source.End)
        {
        }
        public InclusiveRange(int Start, InclusiveRange Source)
            : this(Source)
        {
            this.Start = Start;
        }
        public InclusiveRange(InclusiveRange Source, int End)
            : this(Source)
        {
            this.End = End;
        }
        public InclusiveRange(Range Range)
            :this()
        {
            GetValuesFromRange(Range, out Start, out End);
        }
        public InclusiveRange(int Adjustment, Range Range)
            : this()
        {
            GetValuesFromRange(Range, out Start, out End);
            Start += Adjustment;
            End += Adjustment;
        }
        public InclusiveRange(DieRoll DieRoll)
            : this()
        {
            GetValuesFromDieRoll(DieRoll, out Start, out End);
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
                GetValuesFromDieRoll(dieRoll, out Start, out End);
            }
            else
            {
                ArgumentException argEx = new("Failed to parse argument \"" + Value + "\" into valid " + nameof(Start) + " and " + nameof(End) + " values.", nameof(Value));

                if (!splitString.IsNullOrEmpty())
                {
                    if (Value.Split(splitString) is not string[] values
                        || !int.TryParse(values[0], out Start)
                        || !int.TryParse(values[0], out End))
                        throw argEx;
                }
                else
                if (int.TryParse(Value, out int value))
                {
                    Start = 0;
                    End = value;
                }
                else
                    throw argEx;
            }
        }

        public readonly void Deconstruct(out int Start, out int End)
        {
            Start = this.Start;
            End = this.End;
        }
        public readonly void Deconstruct(out IEnumerable<int> Values)
        {
            Values = GetValues();
        }

        #region Indexer Overloads

        private readonly ArgumentOutOfRangeException New_Indexer_ArgumentOutOfRangeException(object IndexValue, string ParamName)
            => new(
                paramName: ParamName,
                message: "Argument (" + IndexValue + ") must be greater than or equal to 0 and less than the absolute length (" + AbsLength + ") of the range.");

        public readonly int this[int Index]
        {
            get
            {
                if (Index < 0
                    || Index >= AbsLength)
                    throw New_Indexer_ArgumentOutOfRangeException(IndexValue: Index, ParamName: nameof(Index));

                return Start + (Index * Direction);
            }
        }
        public readonly int this[Index Index]
        {
            get
            {
                if (!IsValidIndex(Index))
                    throw New_Indexer_ArgumentOutOfRangeException(IndexValue: Index, ParamName: nameof(Index));

                return !Index.IsFromEnd
                    ? Start + (Index.GetIntValue() * Direction)
                    : End - (Index.GetIntValue() * Direction);
            }
        }
        public readonly IEnumerable<int> this[Range Range]
        {
            get
            {
                if (!IsValidIndex(Range.Start))
                    throw New_Indexer_ArgumentOutOfRangeException(
                        IndexValue: Range.Start,
                        ParamName: CallChain(nameof(System.Range), nameof(System.Range.Start)));

                if (!IsValidIndex(Range.End))
                    throw New_Indexer_ArgumentOutOfRangeException(
                        IndexValue: Range.End,
                        ParamName: CallChain(nameof(System.Range), nameof(System.Range.End)));

                using InclusiveRange subRange = new(Start + Range.Start.GetIntValue(), Start + Range.End.GetIntValue());
                return subRange.GetValues();
            }
        }

        #endregion
        #region Manual Conversions

        public static void GetValuesFromRange(Range Range, out int Start, out int End)
        {
            Start = Range.Start.GetIntValue();
            End = Range.End.GetIntValue();
        }
        public static InclusiveRange GetFromRange(Range Range)
        {
            GetValuesFromRange(Range, out int start, out int end);
            return new(start, end);
        }

        public static void GetValuesFromDieRoll(DieRoll DieRoll, out int Start, out int End)
        {
            if (DieRoll.IsConstantOnlyRangeType())
            {
                Start = DieRoll.LeftValue;
                End = DieRoll.RightValue;
            }
            else
            {
                Start = DieRoll.Min();
                End = DieRoll.Max();
            }
        }
        public static InclusiveRange GetFromDieRoll(DieRoll DieRoll)
        {
            GetValuesFromDieRoll(DieRoll, out int start, out int end);
            return new InclusiveRange(start, end);
        }

        #endregion

        public readonly string ToString(bool Full)
            => Min + "_" + Max + (Full ? ":" + Start + "," + End : null);

        public override readonly string ToString()
            => ToString(true);

        public readonly bool Contains(int Value)
            => Min <= Value
            && Value <= Max;

        private readonly bool IsValidIndex(Index Index)
            => (!Index.IsFromEnd
                && Index.Value >= AbsLength)
            || (Index.IsFromEnd
                && Index.Value > AbsLength);

        public readonly InclusiveRange Clamp(int Min, int Max)
            => IsForward
            ? new InclusiveRange(this.Min.Clamp(Min, Max), this.Max.Clamp(Min, Max))
            : new InclusiveRange(this.Max.Clamp(Min, Max), this.Min.Clamp(Min, Max));

        public readonly InclusiveRange Clamp(InclusiveRange OtherRange)
            => Clamp(OtherRange.Start, OtherRange.End);

        public readonly InclusiveRange AdjustBy(int Value)
            => new(Start + Value, End + Value);

        public readonly InclusiveRange AdjustByValues(InclusiveRange OtherRange)
            => new(Start + OtherRange.Start, End + OtherRange.End);

        public readonly InclusiveRange AdjustByLength(InclusiveRange OtherRange)
            => AdjustBy(OtherRange.Length);

        public readonly InclusiveRange AdjustByClamped(int Value, InclusiveRange Clamp)
            => AdjustBy(Value).Clamp(Clamp);

        public readonly InclusiveRange AdjustByClamped(InclusiveRange OtherRange, InclusiveRange Clamp)
            => AdjustByValues(OtherRange).Clamp(Clamp);

        public readonly IEnumerable<int> GetValues(int Offset, int Step)
        {
            for (int i = 0; i < AbsLength; i++)
                yield return (Offset + Start + i) * Step * Direction;
        }
        public readonly IEnumerable<int> GetValues(int Offset)
            => GetValues(Offset, 1);

        public readonly IEnumerable<int> GetValues()
            => GetValues(0);

        public readonly Range ToRange()
            => Start..End;

        public readonly DieRoll ToDieRoll()
            => new(DieRoll.TYPE_RANGE, Min, Max);

        public readonly int[] ToArray()
            => GetValues()?.ToArray() ?? new int[0];

        public override readonly bool Equals(object obj)
        {
            if (obj is InclusiveRange inclusiveRangeObj)
                Equals(inclusiveRangeObj);

            return base.Equals(obj);
        }
        public readonly bool Equals(InclusiveRange Other)
        {
            if (EitherNull(this, Other, out bool areEqual))
                return areEqual;

            return Start.Equals(Other.Start)
                && End.Equals(Other.End);
        }
        public override readonly int GetHashCode()
            => Start.GetHashCode()
            ^ End.GetHashCode();

        public readonly int Sum(bool IncludeIntermediateValues)
            => !IncludeIntermediateValues
            ? Start + End
            : this.Aggregate(0, (a, n) => a + n);

        public readonly int Sum()
            => Sum(false);

        public readonly int Average(bool IncludeIntermediateValues)
            => !IncludeIntermediateValues
            ? Utils.Average(Start, End)
            : Utils.Average((int[])this);

        public readonly int Average()
            => Average(false);

        public readonly int Breadth()
            => AbsLength;

        public readonly int CompareSum(InclusiveRange Other, bool IncludeIntermediateValues)
            => Sum(IncludeIntermediateValues) - Other.Sum(IncludeIntermediateValues);

        public readonly int CompareSum(InclusiveRange Other)
            => CompareSum(Other, false);

        public readonly int CompareAverage(InclusiveRange Other, bool IncludeIntermediateValues)
            => Average(IncludeIntermediateValues) - Other.Average(IncludeIntermediateValues);

        public readonly int CompareAverage(InclusiveRange Other)
            => CompareAverage(Other, false);

        public readonly int CompareBreadth(InclusiveRange Other)
            => Breadth() - Other.Breadth();

        public readonly int CompareMin(InclusiveRange Other)
            => Min - Other.Min;

        public readonly int CompareMax(InclusiveRange Other)
            => Max - Other.Max;

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

        public void Dispose()
        {
            Start = default;
            End = default;
        }

        public readonly void WriteOptimized(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Start);
            Writer.WriteOptimized(End);
        }

        public static void ReadOptimizedInclusiveRange(SerializationReader Reader, out int Start, out int End)
        {
            Start = Reader.ReadOptimizedInt32();
            End = Reader.ReadOptimizedInt32();
        }
        public static InclusiveRange ReadOptimizedInclusiveRange(SerializationReader Reader)
        {
            ReadOptimizedInclusiveRange(Reader, out int start, out int end);
            return new(start, end);
        }

        public readonly void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedInclusiveRange(Reader, out Start, out End);
        }

        #region Operator Overloads

        #region Equality

        public static bool operator ==(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.Equals(Operand2);

        public static bool operator !=(InclusiveRange Operand1, InclusiveRange Operand2)
            => !(Operand1 == Operand2);

        #endregion
        #region Comparison

        public static bool operator >(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) > 0;

        public static bool operator <(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) < 0;

        public static bool operator >=(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) >= 0;

        public static bool operator <=(InclusiveRange Operand1, InclusiveRange Operand2)
            => Operand1.CompareTo(Operand2) <= 0;

        #endregion
        #region Cast

        public static explicit operator int[](InclusiveRange InclusiveRange)
            => InclusiveRange.ToArray();

        public static implicit operator InclusiveRange(Range Range)
            => new(Range);

        #endregion

        #endregion
    }
}
