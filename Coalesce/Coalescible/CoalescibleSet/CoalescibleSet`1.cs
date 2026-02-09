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

        private EqualityComparer<T> _EqualityComparer;
        public EqualityComparer<T> EqualityComparer
        {
            get => _EqualityComparer ??= EqualityComparer<T>.Default;
            protected set
            {
                _EqualityComparer = value;
                Variant++;
            }
        }

        private Coalescer<T> _Coalescer;
        public Coalescer<T> Coalescer
        {
            get => _Coalescer ??= Coalescer<T>.Default;
            protected set
            {
                _Coalescer = value;
                Variant++;
            }
        }

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

            _EqualityComparer = null;
            _Coalescer = null;

            EnsureCapacity(DefaultCapacity);
        }
        public CoalescibleSet(int Capacity, EqualityComparer<T> EqualityComparer, Coalescer<T> Coalescer)
            : this()
        {
            EnsureCapacity(Capacity);
            _EqualityComparer = EqualityComparer;
            _Coalescer = Coalescer;
        }
        public CoalescibleSet(int Capacity)
            : this(Capacity, null, null)
        { }
        public CoalescibleSet(EqualityComparer<T> EqualityComparer, Coalescer<T> Coalescer)
            : this(0, EqualityComparer, Coalescer)
        { }
        public CoalescibleSet(IReadOnlyList<T> List, EqualityComparer<T> EqualityComparer, Coalescer<T> Coalescer)
            : this(List?.Count ?? 0, EqualityComparer, Coalescer)
        {
            AddRange(List);
        }
        public CoalescibleSet(IReadOnlyList<T> List)
            : this(List, null, null)
        { }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteObject(EqualityComparer);
            Writer.Write(Coalescer);
        }
        public virtual void Read(SerializationReader Reader)
        {
            _EqualityComparer = Reader.ReadObject() as EqualityComparer<T>;
            _Coalescer = Reader.ReadComposite() as Coalescer<T>;
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
        /// Gets the stored <typeparamref name="T"/> element matching <paramref name="Value"/>, if it exists, and returns the result of calling <see cref="Coalescer{T}.Coalesce(T,T)"/> on the two objects, or returns <paramref name="Value"/> if it doesn't.
        /// </summary>
        /// <remarks>
        /// This method will pass the above two objects with the existing element as the first parameter and <paramref name="Value"/> as the second; the inverse of <see cref="GetCoalescedWith(T)"/>.
        /// </remarks>
        /// <param name="Value">The object to <see cref="Coalescer{T}.Coalesce(T,T)"/> with an equal entry in the current set, if one exists.</param>
        /// <returns>The result of calling <see cref="Coalescer{T}.Coalesce(T,T)"/> on a stored, matching <typeparamref name="T"/> element, if it exists and <paramref name="Value"/>;<br/><paramref name="Value"/>, otherwise.</returns>
        public T GetCoalesceWith(T Value)
            => TryGetIndexOf(Value, out int index)
                && this[index] is T itemAtIndex
            ? Coalescer.Coalesce(itemAtIndex, Value)
            : Value;

        /// <summary>
        /// Gets the stored <typeparamref name="T"/> element matching <paramref name="Value"/>, if it exists, and returns the result of calling <see cref="Coalescer{T}.Coalesce(T,T)"/> on the two objects, or returns <paramref name="Value"/> if it doesn't.
        /// </summary>
        /// <remarks>
        /// This method will pass the above two objects with <paramref name="Value"/> as the first parameter and the existing element as the second; the inverse of <see cref="GetCoalesceWith(T)"/>.
        /// </remarks>
        /// <param name="Value">The object to have <see cref="Coalescer{T}.Coalesce(T,T)"/> an equal entry in this <see cref="CoalescibleSet{T}"/> if one exists.</param>
        /// <returns>The result of calling <see cref="Coalescer{T}.Coalesce(T,T)"/> on <paramref name="Value"/> and a stored, matching <typeparamref name="T"/> element, if it exists;<br/><paramref name="Value"/>, otherwise.</returns>
        public T GetCoalescedWith(T Value)
            => TryGetIndexOf(Value, out int index)
                && this[index] is T itemAtIndex
            ? Coalescer.Coalesce(itemAtIndex, Value)
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
            EqualityComparer = null;
            Coalescer = null;
        }

        #endregion
        #region User-defined Conversions

        public static implicit operator ReadOnlySpan<T>(CoalescibleSet<T> CoalescibleSet)
            => CoalescibleSet.AsSpan();

        #endregion
    }
}
