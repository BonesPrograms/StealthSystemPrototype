using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public partial class Perceptions : IComposite
    {
        private Perception[] Items = Array.Empty<Perception>();

        protected int Size;

        protected int Length;

        protected int Variant;

        public int Capacity => Size;

        public int Version => Variant;

        protected virtual int DefaultCapacity => 4;

        public bool WantFieldReflection => false;

        #region Constructors

        public Perceptions()
        {
            Size = 0;
            EnsureCapacity(DefaultCapacity);
            Length = 0;
            Variant = 0;
        }
        public Perceptions(int Capacity)
            : this()
        {
            EnsureCapacity(Capacity);
        }
        public Perceptions(IReadOnlyCollection<Perception> Source)
            : this(Source.Count)
        {
            Length = Source.Count;
        }
        public Perceptions(IEnumerable<Perception> Source)
            : this((IReadOnlyCollection<Perception>)Source)
        {
        }
        public Perceptions(Perceptions Source)
            : this((IReadOnlyCollection<Perception>)Source)
        {
        }

        #endregion

        public void EnsureCapacity(int Capacity)
        {
            if (Size < Capacity)
                Resize(Capacity);
        }

        protected void Resize(int Capacity)
        {
            if (Capacity == 0)
                Capacity = DefaultCapacity;

            Array.Resize(array: ref Items, Capacity);
            Size = Capacity;
        }

        public virtual bool ContainsType(Type Type)
        {
            for (int i = 0; i < Length; i++)
                if (Items[i].GetType() == Type)
                    return true;

            return false;
        }
        public virtual bool ContainsType(Perception Item)
            => ContainsType(Item.GetType());

        public virtual bool Contains<T>(T Item)
            where T : Perception
            => ContainsType(typeof(T));

        #region Serialization
        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Size);
            Writer.WriteOptimized(Length);
            Writer.WriteOptimized(Variant);
            for (int i = 0; i < Length; i++)
                Writer.WriteObject(Items[i]);
        }

        public virtual void Read(SerializationReader Reader)
        {
            Size = Reader.ReadOptimizedInt32();
            Length = Reader.ReadOptimizedInt32();
            Variant = Reader.ReadOptimizedInt32();

        }
        #endregion
    }
}
