using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    [Serializable]
    internal class NullableCoalescer<T> : Coalescer<T?> where T : struct, ICoalescible<T>
    {
        public NullableCoalescer()
            : base()
        { }

        public override T? CoalesceCombine(T? X, T? Y)
            => throw new NotSupportedException();

        public override T? CoalesceDifference(T? X, T? Y)
            => throw new NotSupportedException();

        public override T? CoalesceGreater(T? X, T? Y)
        {
            if (EitherNull(X, Y, out int comparison))
                return comparison < 0
                    ? Y
                    : X;

            if (Y is IComparable<T?> yComparableT)
                return yComparableT.CompareTo(X) < 0
                    ? Y
                    : X;

            if (Y is IComparable yComparable)
                return yComparable.CompareTo(X) > 0
                    ? Y
                    : X;

            throw new InvalidOperationException(typeof(T?).Name + " is neither an " + nameof(IComparable) + " or " + nameof(IComparable<T?>) + ".");
        }

        public override T? CoalesceLesser(T? X, T? Y)
        {
            if (EitherNull(X, Y, out int comparison))
                return comparison > 0
                    ? Y
                    : X;

            if (Y is IComparable<T?> yComparableT)
                return yComparableT.CompareTo(X) < 0
                    ? Y
                    : X;

            if (Y is IComparable yComparable)
                return yComparable.CompareTo(X) < 0
                    ? Y
                    : X;

            throw new InvalidOperationException(typeof(T?).Name + " is neither an " + nameof(IComparable) + " or " + nameof(IComparable<T?>) + ".");
        }
    }
}
