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
        public GameObject Owner;

        private BasePerception[] Items = Array.Empty<BasePerception>();

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
            Owner = null;
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
        public Perceptions(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public Perceptions(GameObject Owner, int Capacity)
            : this(Owner)
        {
            EnsureCapacity(Capacity);
        }
        public Perceptions(IReadOnlyCollection<BasePerception> Source)
            : this(Source.Count)
        {
            Length = Source.Count;
        }
        public Perceptions(GameObject Owner, IReadOnlyCollection<BasePerception> Source)
            : this(Source)
        {
            this.Owner = Owner;
        }
        public Perceptions(IEnumerable<BasePerception> Source)
            : this((IReadOnlyCollection<BasePerception>)Source)
        {
        }
        public Perceptions(GameObject Owner, IEnumerable<BasePerception> Source)
            : this(Owner, (IReadOnlyCollection<BasePerception>)Source)
        {
        }
        public Perceptions(Perceptions Source)
            : this(Source.Owner, Source)
        {
        }

        #endregion

        public virtual string ToString(string Delimiter, bool Short, bool WithRolls)
            => this.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.ToString(Short: Short, WithRoll: WithRolls));

        public virtual string ToString(bool Short, bool WithRolls = false)
            => ToString(", ", Short, WithRolls);

        public virtual string ToStringLines(bool Short = false, bool WithRolls = false)
            => ToString("\n", Short, WithRolls);

        public override string ToString()
            => ToString(Short: false, WithRolls: false);

        public bool Validate(GameObject Owner = null, bool RemoveInvalid = true)
        {
            Owner ??= this.Owner;
            bool allValid = true;
            List<BasePerception> removeList = new();
            for (int i = 0; i < Count; i++)
            {
                if (Items[i] is not BasePerception perception)
                    throw new InvalidOperationException(nameof(Items) + " contains null entry at " + i + " despite length of " + Count);

                if (!perception.Validate(Owner))
                {
                    if (RemoveInvalid)
                        removeList.Add(perception);
                    else
                        allValid = false;
                }
            }
            foreach (BasePerception perception in removeList)
                RemoveType(perception);

            removeList.Clear();

            if (RemoveInvalid
                && !allValid)
                allValid = Validate(Owner, false);

            return allValid;
        }

        #region Container Helpers

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
        public virtual bool ContainsType(BasePerception Item)
            => ContainsType(Item.GetType());

        public virtual bool Contains<T>(T Item)
            where T : BasePerception
            => ContainsType(typeof(T));

        #endregion

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
