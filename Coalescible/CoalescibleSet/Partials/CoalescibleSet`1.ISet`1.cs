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

namespace StealthSystemPrototype
{
    [DebuggerDisplay("Count = {Count}")]
    //[Serializable]
    public abstract partial class CoalescibleSet<T>
        : IComposite
    //  , IDisposable
    //  , IEnumerable<T>
    //  , ICollection<T>
    //  , IReadOnlyCollection<T>
        , ISet<T>
    //  , IList
    //  , IList<T>
    //  , IReadOnlyList<T>
        where T
        : ICoalescible<T>
        , IComposite
    {
        protected void InternalAdd(T Item)
        {
            if (TryGetIndexOf(Item, out int index)
                && Items[index] is T itemAtIndex)
                throw new InvalidOperationException(
                    message: "A " + typeof(CoalescibleSet<T>).ToStringWithGenerics() + " (" +
                        GetType().ToStringWithGenerics() + ") cannot contain duplicate entries. " +
                        nameof(Item) + " (" + Item.ToString() + ") already present (" + itemAtIndex.ToString() + ").");

            EnsureCapacity(Length + 1);
            Items[Length++] = Item;
            Variant++;
        }
        public bool Add(T Item)
        {
            T itemToAdd = Item;
            if (TryGetIndexOf(Item, out int index)
                && Items[index] is T itemAtIndex)
            {
                RemoveAt(index);
                itemToAdd = itemAtIndex.Coalesce(itemToAdd);
            }
            InternalAdd(itemToAdd);
            return true;
        }

        public void ExceptWith(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public void IntersectWith(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSubsetOf(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }

        public void UnionWith(IEnumerable<T> Other)
        {
            throw new NotImplementedException();
        }
    }
}
