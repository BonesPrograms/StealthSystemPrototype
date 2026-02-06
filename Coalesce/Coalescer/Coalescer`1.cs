using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class Coalescer<T> : ICoalescer, ICoalescer<T>
    {
        [Serializable]
        public enum CoalesceMethod
        {
            First,
            Second,
            Greater,
            Lesser,
            Combine,
            Difference,
        }

        private static volatile Coalescer<T> _DefaultCoalescer;

        public static Coalescer<T> Default
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Coalescer<T> coalescer = _DefaultCoalescer;
                coalescer ??= (_DefaultCoalescer = CreateCoalescer());
                return coalescer;
            }
        }

        private CoalesceMethod _Method;

        public CoalesceMethod Method => _Method;

        protected Coalescer()
        {
            _Method = CoalesceMethod.First;
        }
        public Coalescer(CoalesceMethod Method)
        {
            _Method = Method;
        }

        [SecuritySafeCritical]
        private static Coalescer<T> CreateCoalescer()
        {
            Type type = typeof(T);

            if (type == typeof(byte))
                return new ByteCoalescer() as Coalescer<T>;

            if (typeof(ICoalescible<T>).IsAssignableFrom(type))
                return Activator.CreateInstance(typeof(GenericCoalescer<>).MakeGenericType(type)) as Coalescer<T>;

            if (type.IsGenericType
                && type.GetGenericTypeDefinition() == typeof(Nullable<>)
                && type.GetGenericArguments()[0] is Type nullableType
                && typeof(ICoalescible<>).MakeGenericType(nullableType).IsAssignableFrom(nullableType))
                return Activator.CreateInstance(typeof(NullableCoalescer<>).MakeGenericType(nullableType)) as Coalescer<T>;

            if (type.IsEnum
                && Type.GetTypeCode(Enum.GetUnderlyingType(type)) is TypeCode typeCode)
                return Activator.CreateInstance(typeof(EnumCoalescer<,>).MakeGenericType(
                    typeArguments: new Type[]
                    {
                        type,
                        typeCode switch
                        {
                            TypeCode.Int16 => typeof(short),
                            TypeCode.SByte => typeof(sbyte),
                            TypeCode.Byte => typeof(byte),
                            TypeCode.UInt16 => typeof(ushort),
                            TypeCode.Int32 => typeof(int),
                            TypeCode.UInt32 => typeof(uint),
                            TypeCode.Int64 => typeof(long),
                            TypeCode.UInt64 => typeof(ulong),
                            _ => throw new InvalidCastException((int)typeCode + " is not a valid " + nameof(TypeCode) + "."),
                        }
                    })) as Coalescer<T>;

            return new ObjectCoalescer<T>();
        }

        public virtual T CoalesceFirst(T X, T Y)
            => X;

        public virtual T CoalesceSecond(T X, T Y)
            => Y;

        public abstract T CoalesceGreater(T X, T Y);

        public abstract T CoalesceLesser(T X, T Y);

        public abstract T CoalesceCombine(T X, T Y);

        public abstract T CoalesceDifference(T X, T Y);

        public virtual T Coalesce(T X, T Y)
            => Method switch
            {
                CoalesceMethod.First => CoalesceFirst(X, Y),
                CoalesceMethod.Second => CoalesceSecond(X, Y),
                CoalesceMethod.Greater => CoalesceGreater(X, Y),
                CoalesceMethod.Lesser => CoalesceLesser(X, Y),
                CoalesceMethod.Combine => CoalesceCombine(X, Y),
                CoalesceMethod.Difference => CoalesceDifference(X, Y),
                _ => throw new InvalidOperationException((int)Method + " is not a valid value for " + nameof(Method) + "."),
            };

        object ICoalescer.Coalesce(object X, object Y)
        {
            if (X is not T tObject)
                throw new ArgumentException("Invalid type  to " + nameof(Coalesce), nameof(X));
            if (Y is not T tOther)
                throw new ArgumentException("Invalid type  to " + nameof(Coalesce), nameof(X));

            return Coalesce(tObject, tOther);
        }
    }
}
