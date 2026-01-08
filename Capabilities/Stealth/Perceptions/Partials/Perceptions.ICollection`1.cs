using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public partial class Perceptions : ICollection<Perception>
    {
        public int Count => Length;

        public bool IsReadOnly => false;

        private void Add(Perception Item)
        {
            if (Length >= Size)
                Resize(Length + 1);

            Items[Length++] = Item;
            Variant++;
        }

        public void Add<T>(T Item, bool Override)
            where T : Perception, new()
        {
            if (Items == null)
                throw new Exception(nameof(Items) + " is null when it shouldn't be");

            if (Contains(Item)
                && Override)
            {
                Remove(Item);
                Add(Item);
            }
            else
            {
                Add(Item);
            }
        }

        public void Clear()
        {
            Array.Clear(Items, 0, Items.Length);
            Size = 0;
            Length = 0;
            Variant = 0;
        }

        public void CopyTo(Perception[] Array, int ArrayIndex)
            => Items.CopyTo(Array, ArrayIndex);

        public bool Remove<T>(T Item)
            where T : Perception, new()
        {
            if (Item == null)
                throw new ArgumentNullException(nameof(Item), "cannot be null");
            
            int index = -1;
            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == typeof(T))
                {
                    index = i;
                    break;
                }

            if (index < 0)
                return false;

            Length--;

            if (index < Count)
                Array.Copy(Items, index + 1, Items, index, Count - index);

            Items[Count] = null;
            Variant++;

            return true;
        }

        #region Explicit Implementations

        bool ICollection<Perception>.Contains(Perception Item)
            => throw new NotImplementedException("This container's items should be uniquely typed");

        bool ICollection<Perception>.Remove(Perception item)
            => throw new NotImplementedException("This container's items should be uniquely typed");

        void ICollection<Perception>.Add(Perception item)
            => throw new NotImplementedException("This container's items should be uniquely typed");

        #endregion
    }
}
