using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.Collections;

using SerializeField = UnityEngine.SerializeField;

using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.AlertExtensions;

using Debug = StealthSystemPrototype.Logging.Debug;
using System.Runtime.CompilerServices;

namespace StealthSystemPrototype
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public partial class CoalescibleSet<T>
        : IComposite
        , IDisposable
    //  , IEnumerable<T>
    //  , ICollection
    //  , ICollection<T>
    //  , IReadOnlyCollection<T>
    //  , ISet<T>
    //  , IList
    //  , IList<T>
    //  , IReadOnlyList<T>
        where T
        : IComposite
    {
        #region Debug
        /*
        [UD_DebugRegistry]
        public static void CoalescibleSet_DoDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.CoalescibleSet<T>),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(Add), false },
                });
        }
        */
        #endregion
        #region Instance Fields & Properties

        protected T[] Items = Array.Empty<T>();

        protected int Length;
        protected int Size;
        protected int Variant;

        private EqualityComparer<T> _Comparer;
        public EqualityComparer<T> Comparer => _Comparer;

        private Coalescer<T> _Coalescer;
        public Coalescer<T> Coalescer => _Coalescer;

        public int Capacity => Size;
        public virtual int DefaultCapacity => 4;
        public int Version => Variant;

        public bool WantFieldReflection => false;

        #endregion
        #region Constructors

        public CoalescibleSet()
        {
            Length = 0;
            Size = 0;
            Variant = 0;

            _Comparer = null;
            _Coalescer = null;
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Length);
            for (int i = 0; i < Length; i++)
                Writer.Write(Items[i]);
        }

        public virtual void Read(SerializationReader Reader)
        {
            Size = (Length = Reader.ReadOptimizedInt32());
            Items = new T[Size];
            for (int i = 0; i < Length; i++)
                Items[i] = (T)Reader.ReadComposite();
        }

        #endregion
        #region Collection Helpers

        protected void Resize(int Capacity)
        {
            if (Capacity == 0)
                Capacity = DefaultCapacity;

            T[] array = new T[Capacity];
            Array.Copy(Items, 0, array, 0, Length);
            Items = array;
            Size = Capacity;
        }
        public void EnsureCapacity(int Capacity)
        {
            if (Size < Capacity)
                Resize(Capacity);
        }

        public void AddRange(IEnumerable<T> Items)
        {
            int count = Items.Count();
            EnsureCapacity(Length + count);
            Items.ForEach(e => Add(e));
        }

        public void AddRange(IReadOnlyCollection<T> Items)
            => AddRange(Items as IEnumerable<T>);

        public void AddRange(IReadOnlyList<T> Items)
            => AddRange(Items as IReadOnlyCollection<T>);

        public void AddRange(ReadOnlySpan<T> Items)
        {
            if (Items.GetEnumerator() is ReadOnlySpan<T>.Enumerator enumerator)
            {
                EnsureCapacity(Length + Items.Length);
                while (enumerator.MoveNext())
                    Add(enumerator.Current);
            }
        }

        public bool TryGetIndexOf(T Value, out int Index)
            => (Index = IndexOf(Value)) >= 0;

        public Span<T> FillSpan(int Length)
        {
            EnsureCapacity(this.Length + Length);
            Span<T> result = new(Items, this.Length, Length);
            this.Length += Length;
            Variant++;
            return result;
        }

        public virtual T[] ToArray()
        {
            T[] array = new T[Length];
            Array.Copy(Items, 0, array, 0, Length);
            return array;
        }

        #endregion
        #region Coalesce

        /// <summary>
        /// Gets the stored <typeparamref name="T"/> Item matching <paramref name="Value"/>, if it exists, and calls its <see cref="ICoalescible{T}.Coalesce(T)"/> on <paramref name="Value"/>, returning the result, or returning <paramref name="Value"/> if it doesn't.
        /// </summary>
        /// <param name="Value">The object to <see cref="ICoalescible{T}.Coalesce(T)"/> with an equal entry in this <see cref="CoalescibleSet{T}"/> if one exists.</param>
        /// <returns>The result of the stored <typeparamref name="T"/> Item matching <paramref name="Value"/>, if it exists, calling <see cref="ICoalescible{T}.Coalesce(T)"/> on <paramref name="Value"/>;<br/><paramref name="Value"/>, otherwise.</returns>
        public T GetCoalesceWith(T Value)
            => TryGetIndexOf(Value, out int index)
                && this[index] is T itemAtIndex
            ? itemAtIndex.Coalesce(Value)
            : Value;

        /// <summary>
        /// Gets the stored <typeparamref name="T"/> Item matching <paramref name="Value"/>, if it exists, and calls the passed <paramref name="Value"/>'s <see cref="ICoalescible{T}.Coalesce(T)"/> on it, returning the result, or returning <paramref name="Value"/> if it doesn't.
        /// </summary>
        /// <param name="Value">The object to have <see cref="ICoalescible{T}.Coalesce(T)"/> an equal entry in this <see cref="CoalescibleSet{T}"/> if one exists.</param>
        /// <returns>The result of <paramref name="Value"/> calling <see cref="ICoalescible{T}.Coalesce(T)"/> on a stored, matching <typeparamref name="T"/>, if it exists;<br/><paramref name="Value"/>, otherwise.</returns>
        public T GetCoalescedWith(T Value)
            => TryGetIndexOf(Value, out int index)
                && this[index] is T itemAtIndex
            ? Value.Coalesce(itemAtIndex)
            : Value;

        public bool TryGetCoalesceWith(T Value, out T CoalescedValue)
            => !(CoalescedValue = GetCoalesceWith(Value)).Equals(Value);

        public bool TryGetCoalescedWith(T Value, out T CoalescedValue)
            => !(CoalescedValue = GetCoalescedWith(Value)).Equals(Value);

        #endregion
        #region ReadOnlySpan

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<T> AsSpan(int Start, int Length)
            => (uint)(Start + Length) > (uint)this.Length
            ? throw new ArgumentOutOfRangeException("Length")
            : new(Items, Start, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<T> AsSpan(int Start)
            => (uint)Start > (uint)Length
            ? throw new ArgumentOutOfRangeException("Start")
            : AsSpan(Start, Length - Start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<T> AsSpan()
            => AsSpan(0, Length);

        #endregion
        #region Disposable

        public virtual void Dispose()
        {
            Items = null;
            Length = 0;
            Size = 0;
            Variant = 0;
        }

        #endregion
        #region User-defined Conversions

        public static implicit operator ReadOnlySpan<T>(CoalescibleSet<T> CoalescibleSet)
            => CoalescibleSet.AsSpan();

        #endregion
    }
}
