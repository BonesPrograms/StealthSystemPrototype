using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace StealthSystemPrototype
{
    [Serializable]
    internal class EnumCoalescer<T, U> : Coalescer<T> where T : struct, Enum where U : IComparable
    {
        public EnumCoalescer()
            : base()
        { }
        public EnumCoalescer(CoalesceMethod Method)
            : base(Method)
        { }

        public override T CoalesceCombine(T X, T Y)
            => throw new NotSupportedException();

        public override T CoalesceDifference(T X, T Y)
            => throw new NotSupportedException();

        public override T CoalesceGreater(T X, T Y)
        {
            if (Y is U yComparable)
                return yComparable.CompareTo(X) > 0
                    ? Y
                    : X;

            throw new InvalidOperationException(typeof(T).Name + " is not of " + nameof(Type) + " " + typeof(U).Name + ".");
        }

        public override T CoalesceLesser(T X, T Y)
        {
            if (Y is U yComparable)
                return yComparable.CompareTo(X) < 0
                    ? Y
                    : X;

            throw new InvalidOperationException(typeof(T).Name + " is not of " + nameof(Type) + " " + typeof(U).Name + ".");
        }
    }
}
