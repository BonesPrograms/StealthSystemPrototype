using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

using XRL.World;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    /// <summary>
    /// Represents an inclusive range of <see cref="IComparable{T}"/> type values that are greater than or equal to the assigned <see cref="Breadth{T}.Min"/> value and less than or equal to the <see cref="Breadth{T}.Max"/> value.
    /// </summary>
    /// <remarks>
    /// A <see cref="Breadth{T}"/> with a <see cref="Breadth{T}.Min"/> that compares greater than its <see cref="Breadth{T}.Max"/> is treated as "inside out".<br /><br />
    /// For example:
    /// <list type="bullet">
    /// <item>A <see cref="Breadth{T}"/> of type <see cref="int"/> with <see cref="Breadth{T}.Min"/> assigned 5 and <see cref="Breadth{T}.Max"/> assigned 10 represents any value where <code>(value <see cref="}"/>= 5 &amp;&amp; value <see cref="{"/>=10)</code></item>
    /// <item>A <see cref="Breadth{T}"/> of type <see cref="int"/> with <see cref="Breadth{T}.Min"/> assigned 10 and <see cref="Breadth{T}.Max"/> assigned 5 represents any value where <code>(value <see cref="{"/>=5 || value <see cref="}"/>= 10)</code></item>
    /// </list>
    /// </remarks>
    /// <typeparam name="T">Any comparable type</typeparam>
    public struct Breadth<T> : IEquatable<Breadth<T>>, IComparable<Breadth<T>>
        where T : IComparable
    {
        public T Min;
        public T Max;

        private readonly bool InsideOut => Comparer<T>.Default.Compare(Min, Max) > 0;

        public Breadth(T Min, T Max)
        {
            this.Min = Min;
            this.Max = Max;
        }

        public override readonly string ToString()
        {
            string minString = "(" + Min.ToString();
            string maxString = Max.ToString() + ")";
            return !InsideOut
                ? minString + ".." + maxString
                : maxString + ".." + minString;
        }

        public readonly bool Contains(T Value)
            => !InsideOut
            ? Comparer<T>.Default.Compare(Min, Value) <= 0
                && Comparer<T>.Default.Compare(Value, Max) <= 0
            : Comparer<T>.Default.Compare(Min, Value) <= 0
                || Comparer<T>.Default.Compare(Value, Max) <= 0;

        public int CompareTo(Breadth<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool Equals(Breadth<T> Other)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(Breadth<T> Operand1, Breadth<T> Operand2)
        {
            if (EitherNull(Operand1, Operand2, out bool areEqual))
                return areEqual;

            return Operand1.Min.Equals(Operand2.Min)
                && Operand1.Max.Equals(Operand2.Min);
        }
        public static bool operator !=(Breadth<T> Operand1, Breadth<T> Operand2)
            => !(Operand1 == Operand2);
    }
}
