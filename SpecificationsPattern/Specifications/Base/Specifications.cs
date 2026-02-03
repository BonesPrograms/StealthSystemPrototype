using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract partial class Specifications : Specification
    {
        protected Specification[] Items = Array.Empty<Specification>();

        protected int Size;

        protected int Length;

        protected int Variant;

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        public int Capacity => Size;

        public int Version => Variant;

        public Specifications()
        {
        }

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
            Items = new Specifications[Size];
            for (int i = 0; i < Length; i++)
                Items[i] = Reader.ReadComposite() as Specification;
        }

        #endregion

        public override void Dispose()
        {
            Clear();
        }

        protected virtual void Resize(int Capacity)
        {
            if (Capacity == 0)
            {
                Capacity = DefaultCapacity;
            }
            Specification[] array = new Specification[Capacity];
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

        public virtual Specification[] ToArray()
        {
            Specification[] array = new Specification[Length];
            Array.Copy(Items, 0, array, 0, Length);
            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<Specification> AsSpan(int Start, int Length)
            => (uint)(Start + Length) > (uint)this.Length
            ? throw new ArgumentOutOfRangeException("Length")
            : new(Items, Start, Length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<Specification> AsSpan(int Start)
            => (uint)Start > (uint)Length
            ? throw new ArgumentOutOfRangeException("Start")
            : AsSpan(Start, Length - Start);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public virtual ReadOnlySpan<Specification> AsSpan()
            => AsSpan(0, Length);

        public static implicit operator bool(Specifications Operand)
            => Operand.Check();
    }
}
