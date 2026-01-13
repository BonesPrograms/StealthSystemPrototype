using System;
using System.Collections;
using System.Collections.Generic;
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
    public struct Breadth : IComposite, IEquatable<Breadth>, IComparable<Breadth>, IEnumerable<int>
    {
        [Serializable]
        public struct Enumerator
            : IEnumerator<int>
            , IEnumerator
            , IDisposable
        {
            private Breadth Breadth;
            private int Index;
            public int Current => throw new NotImplementedException();
            object IEnumerator.Current => Current;



            public bool MoveNext()
            {
                throw new NotImplementedException();
            }

            public void Reset()
            {
                throw new NotImplementedException();
            }

            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }

        public static Breadth Empty => new(0, 0);

        [NonSerialized]
        public int Min;
        [NonSerialized]
        public int Max;

        public Breadth(int Min, int Max)
        {
            if (Min > Max)
                throw new ArgumentOutOfRangeException(nameof(Min), "cannot be greater than " + nameof(Max) + ".");

            this.Min = Min;
            this.Max = Max;
        }
        public Breadth(int Min, Breadth Source)
            : this(Min, Source.Max)
        {
        }
        public Breadth(Breadth Source, int Max)
            : this(Source.Min, Max)
        {
        }
        public Breadth(Range Range)
        {
            Min = (Range.Start.IsFromEnd ? -Range.Start.Value : Range.Start.Value);
            Max = (Range.End.IsFromEnd ? Min + Range.End.Value : Range.End.Value);
        }
        public Breadth(int Value, Range Range)
        {
            Min = Value + (Range.Start.IsFromEnd ? -Range.Start.Value : Range.Start.Value);
            Max = Value + (Range.End.IsFromEnd ? -Range.End.Value : Range.End.Value);
        }

        public readonly void Deconstruct(out int Min, out int Max)
        {
            Min = this.Min;
            Max = this.Max;
        }

        public readonly int this[int Index]
        {
            get
            {
                if (Index < 0
                    || Index > Difference())
                    throw new ArgumentOutOfRangeException(nameof(Index), "must be between 0 and the difference between Min and Max, inclusive.");
                return Min + Index;
            }
        }

        public readonly int this[Index Index]
        {
            get
            {
                if (Index.Value < 0
                    || Index.Value > Difference())
                    throw new ArgumentOutOfRangeException(nameof(Index), "must be between 0 and the difference between Min and Max, inclusive.");
                return !Index.IsFromEnd
                    ? Min + Index.Value
                    : Max - Index.Value;
            }
        }

        public readonly int[] this[Range Range]
        {
            get
            {
                if (Range.Start.Value > Difference()
                    || Range.End.Value > Difference())
                    throw new ArgumentOutOfRangeException(nameof(Range), "must be between 0 and the difference between Min and Max, inclusive.");
                int start = this[Range.Start];
                int end = this[Range.End];
                int[] array = new int[(end - start)];
                for (int i = 0; i < end - start; i++)
                    array[i] = i + start;
                return array;
            }
        }

        public override readonly string ToString()
            => Min + ".." + Max;

        public readonly bool Contains(int Value)
            => Min <= Value
            && Value <= Max;

        public readonly Breadth Clamp(int Min, int Max)
            => new(Math.Min(this.Min, Min), Math.Max(this.Max, Max));

        public readonly Breadth Clamp(Breadth Breadth)
            => Clamp(Breadth.Min, Breadth.Max);

        public readonly Breadth AdjustBy(int Value)
            => new(Min + Value, Max + Value);

        public readonly Breadth AdjustBy(Breadth OtherRange)
            => new(Min + OtherRange.Min, Max + OtherRange.Max);

        public readonly Breadth AdjustByClamped(int Value, Breadth Clamp)
            => AdjustBy(Value).Clamp(Clamp);

        public readonly Breadth AdjustByClamped(Breadth OtherRange, Breadth Clamp)
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
            => 0..Difference();

        public override readonly bool Equals(object obj)
        {
            if (obj is Breadth breadthObj)
                Equals(breadthObj);

            return base.Equals(obj);
        }
        public readonly bool Equals(Breadth Other)
        {
            if (EitherNull(this, Other, out bool areEqual))
                return areEqual;

            return Min.Equals(Other.Min)
                && Max.Equals(Other.Min);
        }
        public override readonly int GetHashCode()
            => Min.GetHashCode()
            ^ Max.GetHashCode();

        public readonly int Sum()
            => Min + Max;

        public readonly int Average()
            => Utils.Average(Min, Max);

        public readonly int Difference()
            => Max - Min;

        public readonly int CompareSum(Breadth Other)
            => Sum() - Other.Sum();

        public readonly int CompareAverage(Breadth Other)
            => Average() - Other.Average();

        public readonly int CompareDifference(Breadth Other)
            => Difference() - Other.Difference();

        public readonly int CompareMin(Breadth y)
            => Min - y.Min;

        public readonly int CompareMax(Breadth Other)
            => Min - Other.Min;

        public readonly int CompareMinThenMax(Breadth Other)
            => CompareMin(Other) is int floorComp
                && floorComp == 0
            ? CompareMax(Other)
            : floorComp;

        public readonly int CompareMaxThenMin(Breadth Other)
            => CompareMax(Other) is int ceilingComp
                && ceilingComp == 0
            ? CompareMin(Other)
            : ceilingComp;

        public readonly int CompareTo(Breadth Other)
            => CompareSum(Other)
            + CompareAverage(Other)
            + CompareDifference(Other)
            + CompareMin(Other)
            + CompareMin(Other);

        public IEnumerator<int> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public readonly void WriteOptimized(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Min);
            Writer.WriteOptimized(Max);
        }
        public static Breadth ReadOptimizedBreadth(SerializationReader Reader)
            => new(Reader.ReadOptimizedInt32(), Reader.ReadOptimizedInt32());

        public static void ReadOptimizedBreadth(SerializationReader Reader, out int Min, out int Max)
        {
            Min = Reader.ReadOptimizedInt32();
            Max = Reader.ReadOptimizedInt32();
        }

        public readonly void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedBreadth(Reader, out Min, out Max);
        }

        public static bool operator ==(Breadth Operand1, Breadth Operand2)
            => Operand1.Equals(Operand2);

        public static bool operator !=(Breadth Operand1, Breadth Operand2)
            => !(Operand1 == Operand2);

        public static bool operator >(Breadth Operand1, Breadth Operand2)
            => Operand1.CompareTo(Operand2) > 0;

        public static bool operator <(Breadth Operand1, Breadth Operand2)
            => Operand1.CompareTo(Operand2) < 0;

        public static bool operator >=(Breadth Operand1, Breadth Operand2)
            => Operand1.CompareTo(Operand2) >= 0;

        public static bool operator <=(Breadth Operand1, Breadth Operand2)
            => Operand1.CompareTo(Operand2) <= 0;
    }
}
