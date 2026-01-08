using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    public partial class Perceptions : IList<Perception>
    {
        public int IndexOf(Perception Item)
            => Array.IndexOf(Items, Item, 0, Count);

        public int IndexOf<T>(T Item)
            where T : Perception, new()
        {
            if (Items == null)
                throw new Exception(nameof(Items) + " is null when it shouldn't be");

            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == Item.GetType())
                    return i;
            return -1;
        }

        #region Explicit Implementations

        Perception IList<Perception>.this[int index]
        {
            get => throw new NotImplementedException("The order of this collection is arbitrary");
            set => throw new NotImplementedException("This container's items should be uniquely typed");
        }

        public void Insert(int index, Perception item)
            => throw new NotImplementedException("The order of this collection is arbitrary");

        void IList<Perception>.RemoveAt(int Index)
            => throw new NotImplementedException("The order of this collection is arbitrary");

        #endregion
    }
}
