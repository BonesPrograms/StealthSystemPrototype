using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Collections;
using XRL.World;

using static StealthSystemPrototype.Capabilities.Stealth.BasePerception;
using static StealthSystemPrototype.Utils;

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
        public Perceptions(GameObject NewOwner, Perceptions Source)
            : this(NewOwner, (IReadOnlyCollection<BasePerception>)Source)
        {
        }
        public Perceptions(Perceptions Source)
            : this(Source.Owner, Source)
        {
        }

        #endregion

        public virtual string ToString(string Delimiter, bool Short, GameObject Entity, bool UseLastRoll = false)
            => Items.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.ToString(Short: Short, Entity: Entity, UseLastRoll: UseLastRoll));

        public virtual string ToString(bool Short, GameObject Entity = null, bool UseLastRoll = false)
            => ToString(", ", Short, Entity, UseLastRoll);

        public virtual string ToStringLines(bool Short = false, GameObject Entity = null, bool UseLastRoll = false)
            => ToString("\n", Short, Entity, UseLastRoll);

        public override string ToString()
            => ToString(Short: false, Entity: null);

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

        public void ClearRatings()
        {
            for (int i = 0; i < Count; i++)
                Items[i].SetWantsToClearRating();
        }

        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity,
            Predicate<BasePerception> Filter,
            bool ClearFirst)
        {
            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            if (Items.ToList() is not List<BasePerception> perceptionsList)
                return null;

            if (ClearFirst)
                ClearRatings();

            perceptionsList.Sort(new RatingComparer(Entity));

            return perceptionsList
                ?.Where(Filter.ToFunc());
        }
        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity,
            Predicate<BasePerception> Filter)
            => GetPerceptionsBestFirst(Entity, Filter, true);

        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity,
            bool ClearFirst)
            => GetPerceptionsBestFirst(Entity, null, ClearFirst);

        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity)
            => GetPerceptionsBestFirst(Entity, null);

        public BasePerception GetHighestRatedPerceptionFor(GameObject Entity, bool ClearFirst)
        {
            if (Entity == null
                || GetPerceptionsBestFirst(Entity, ClearFirst) is not List<BasePerception> highestFirstList
                || highestFirstList.Count < 1)
                return null;

            return highestFirstList[0];
        }
        public BasePerception GetHighestRatedPerceptionFor(GameObject Entity)
            => GetHighestRatedPerceptionFor(Entity, true);

        public virtual int Roll(GameObject Entity, out BasePerception Perception)
        {
            int highest = -1;
            Perception = null;

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            for (int i = 0; i < Length; i++)
            {
                int roll = Items[i].Roll(Entity);
                if (roll > highest)
                {
                    highest = roll;
                    Perception = Items[i];
                }
            }
            return highest;
        }
        public virtual int Roll(GameObject Entity)
            => Roll(Entity, out _);

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll, out BasePerception Perception)
        {
            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(GetAwareness) + " requires a " + nameof(GameObject) + " to perceive.");

            Roll = this.Roll(Entity, out Perception);

            return Perception.GetAwareness(Entity, UseLastRoll: true);
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out BasePerception Perception)
            => GetAwareness(Entity, out _, out Perception);

        public virtual AwarenessLevel GetAwareness(GameObject Entity)
            => GetAwareness(Entity, out _, out _);

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

        #region Conversion Methods

        public IEnumerable<BasePerception> AsEnumerable(Predicate<BasePerception> Filter = null)
        {
            try
            {
                if (Items == null)
                    throw new InnerArrayNullException(nameof(Items));

                return Items.Where(Filter?.ToFunc());
            }
            catch (InnerArrayNullException)
            {
                return new BasePerception[0];
            }
        }

        public IList<BasePerception> ToList()
            => ToCollection()?.ToList();

        public ICollection<BasePerception> ToCollection(Predicate<BasePerception> Filter = null)
            => ToList();

        public BasePerception[] ToArray()
            => AsEnumerable() as BasePerception[];

        #endregion

        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Size);
            Writer.WriteOptimized(Length);
            Writer.WriteOptimized(Variant);
            for (int i = 0; i < Length; i++)
                Writer.Write(Items[i]);
        }

        public virtual void Read(SerializationReader Reader)
        {
            Size = Reader.ReadOptimizedInt32();
            Length = Reader.ReadOptimizedInt32();
            Variant = Reader.ReadOptimizedInt32();
            Items = new BasePerception[Size];
            for (int i = 0; i < Length; i++)
                Items[i] = Reader.ReadComposite() as BasePerception;
        }

        #endregion
    }
}
