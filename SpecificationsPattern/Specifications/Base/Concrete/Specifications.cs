using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract partial class Specifications : ISpecification
    {
        protected ISpecification[] Items = Array.Empty<ISpecification>();

        protected int Size;

        protected int Length;

        protected int Variant;

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        public int Capacity => Size;

        public int Version => Variant;

        public Specifications()
            : base()
        {
            Size = 0;
            Length = 0;
            Variant = 0;
        }
        public Specifications(IReadOnlyList<ISpecification> List)
            : this()
        {
            if (List != null)
                AddRange(List);
        }
        public Specifications(Specifications Source)
            : this(Source as IReadOnlyList<ISpecification>) { }

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
            Items = new Specification[Size];
            for (int i = 0; i < Length; i++)
                Items[i] = Reader.ReadComposite() as Specification;
        }

        #endregion

        public abstract bool Check();

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
            ISpecification[] array = new ISpecification[Capacity];
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
        public virtual void AddRange(IReadOnlyList<ISpecification> Items)
        {
            if (Items == null)
                throw new ArgumentNullException(nameof(Items));

            int count = Items.Count;
            EnsureCapacity(Length + count);
            for (int i = 0; i < count; i++)
                Add(Items[i]);
        }

        public virtual ISpecification[] ToArray()
        {
            ISpecification[] array = new ISpecification[Length];
            Array.Copy(Items, 0, array, 0, Length);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<ISpecification> AsSpan(int Start, int Length)
            => (uint)(Start + Length) > (uint)this.Length
            ? throw new ArgumentOutOfRangeException("Length")
            : new(Items, Start, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<ISpecification> AsSpan(int Start)
            => (uint)Start > (uint)Length
            ? throw new ArgumentOutOfRangeException("Start")
            : AsSpan(Start, Length - Start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<ISpecification> AsSpan()
            => AsSpan(0, Length);

        public static implicit operator bool(Specifications Operand)
            => Operand.Check();
    }
}
