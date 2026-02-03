using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract partial class Specifications<T> : ISpecification<T>
    {
        protected ISpecification<T>[] Items = Array.Empty<ISpecification<T>>();

        protected int Size;

        protected int Length;

        protected int Variant;

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        public int Capacity => Size;

        public int Version => Variant;

        protected T _Subject;
        public virtual T Subject
        {
            get => _Subject;
            set => _Subject = value;
        }

        public Specifications()
        {
            Size = 0;
            Length = 0;
            Variant = 0;
            Subject = default;
        }
        public Specifications(T Subject, IReadOnlyList<ISpecification<T>> List)
            : this()
        {
            this.Subject = Subject;

            if (List != null)
                AddRange(List);
        }
        public Specifications(T Subject)
            : this(Subject, null) { }

        public Specifications(IReadOnlyList<ISpecification<T>> List)
            : this(default, List) { }

        public Specifications(Specifications<T> Source)
            : this(Source.Subject, Source.Items) { }

        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Length);
            for (int i = 0; i < Length; i++)
                Writer.Write(Items[i]);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Size = Length = Reader.ReadOptimizedInt32();
            Items = new Specification<T>[Size];
            for (int i = 0; i < Length; i++)
                Items[i] = Reader.ReadComposite() as ISpecification<T>;
        }

        #endregion

        public virtual bool Check()
            => Check(Subject);
        public abstract bool Check(T Subject);

        public virtual void Dispose()
        {
            Clear();
        }

        protected virtual void Resize(int Capacity)
        {
            if (Capacity == 0)
            {
                Capacity = DefaultCapacity;
            }
            ISpecification<T>[] array = new ISpecification<T>[Capacity];
            Array.Copy(Items, 0, array, 0, Length);
            Items = array;
            Size = Capacity;
        }
        public virtual void EnsureCapacity(int Capacity)
        {
            if (Size < Capacity)
            {
                Resize(Capacity);
            }
        }
        public virtual void AddRange(IReadOnlyList<ISpecification<T>> Items)
        {
            if (Items == null)
                throw new ArgumentNullException(nameof(Items));

            int count = Items.Count;
            EnsureCapacity(Length + count);
            for (int i = 0; i < count; i++)
                Add(Items[i]);
        }

        public virtual ISpecification<T>[] ToArray()
        {
            ISpecification<T>[] array = new ISpecification<T>[Length];
            Array.Copy(Items, 0, array, 0, Length);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<ISpecification<T>> AsSpan(int Start, int Length)
            => (uint)(Start + Length) > (uint)this.Length
            ? throw new ArgumentOutOfRangeException("Length")
            : new(Items, Start, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<ISpecification<T>> AsSpan(int Start)
            => (uint)Start > (uint)Length
            ? throw new ArgumentOutOfRangeException("Start")
            : AsSpan(Start, Length - Start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<ISpecification<T>> AsSpan()
            => AsSpan(0, Length);

        public static explicit operator Specifications(Specifications<T> Operand)
            => (Specifications)Operand.Select(s => s as ISpecification);
    }
}
