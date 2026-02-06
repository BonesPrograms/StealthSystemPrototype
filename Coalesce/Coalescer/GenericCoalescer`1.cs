using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace StealthSystemPrototype
{
    [Serializable]
    internal class GenericCoalescer<T> : Coalescer<T> where T : ICoalescible<T>
    {
        public GenericCoalescer()
            : base()
        { }

        public override T Coalesce(T X, T Y)
            => X.Coalesce(Y);

        public override T CoalesceCombine(T X, T Y)
            => throw new NotSupportedException();

        public override T CoalesceDifference(T X, T Y)
            => throw new NotSupportedException();

        public override T CoalesceGreater(T X, T Y)
        {
            if (Y is IComparable<T> yComparableT)
                return yComparableT.CompareTo(X) > 0
                    ? Y
                    : X;

            if (Y is IComparable yComparable)
                return yComparable.CompareTo(X) > 0
                    ? Y
                    : X;

            throw new InvalidOperationException(typeof(T).Name + " is neither an " + nameof(IComparable) + " or " + nameof(IComparable<T>) + ".");
        }

        public override T CoalesceLesser(T X, T Y)
        {
            if (Y is IComparable<T> yComparableT)
                return yComparableT.CompareTo(X) < 0
                    ? Y
                    : X;

            if (Y is IComparable yComparable)
                return yComparable.CompareTo(X) < 0
                    ? Y
                    : X;

            throw new InvalidOperationException(typeof(T).Name + " is neither an " + nameof(IComparable) + " or " + nameof(IComparable<T>) + ".");
        }
    }
}
