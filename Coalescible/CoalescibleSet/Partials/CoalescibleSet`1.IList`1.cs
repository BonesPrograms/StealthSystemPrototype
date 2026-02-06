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
    //  , ISet<T>
        , IList
        , IList<T>
        , IReadOnlyList<T>
        where T
        : ICoalescible<T>
        , IComposite
    {
        public bool IsFixedSize => false;

        public T this[int Index]
        {
            get => Items[Index];
            set
            {
                if (TryGetIndexOf(value, out int valueIndex)
                    && valueIndex != Index)
                    RemoveAt(Index);
                Add(value);
            }
        }

        object IList.this[int Index]
        {
            get => this[Index];
            set
            {
                if (value is null)
                    Remove(this[Index]);
                else
                if (CheckValidCastOrThrow(value, out T tValue))
                    this[Index] = tValue;
            }
        }

        protected bool CheckValidCastOrThrow(object value, out T TValue)
        {
            TValue = default;
            if (value is null)
                return true;
            else
            if (value is T tValue)
            {
                TValue = tValue;
                return true;
            }
            throw new InvalidCastException(
                message: nameof(value) + ", of " + nameof(Type) + " " + value.GetType().ToStringWithGenerics() +
                    " could not be cast to T " + nameof(Type) + " " + typeof(T).ToStringWithGenerics());
        }

        int IList.Add(object value)
            => CheckValidCastOrThrow(value, out T tValue)
                && Add(tValue)
            ? IndexOf(tValue)
            : -1;

        bool IList.Contains(object value)
            => CheckValidCastOrThrow(value, out T tValue)
                && Contains(tValue);

        public int IndexOf(T item)
            => Items
                .Select((o, i) => new KeyValuePair<int, T>(i, o)) // convert to IEnumerable of KVP<int, T> where int is Index
                .Aggregate(-1, (a, n) // start with -1 (no item)
                    => n.Value.Equals(item) // if (n)ext.Value == Item
                    ? n.Key // (a)ccumulator = n.Key (the Index)
                    : a); // otherwise a is unchanged.

        int IList.IndexOf(object Value)
            => CheckValidCastOrThrow(Value, out T tValue)
            ? IndexOf(tValue)
            : -1;

        void IList<T>.Insert(int Index, T Item)
            => Add(Item);

        void IList.Insert(int Index, object Value)
        {
            if (CheckValidCastOrThrow(Value, out T tValue))
                Add(tValue);
        }

        void IList.Remove(object Value)
        {
            if (CheckValidCastOrThrow(Value, out T tValue))
                Remove(tValue);
        }

        public void RemoveAt(int Index)
        {
            if (Index >= Length)
                throw new ArgumentOutOfRangeException();

            Length--;

            if (Index < Length)
                Array.Copy(Items, Index + 1, Items, Index, Length - Index);

            Items[Length] = default;
            Variant++;
        }
    }
}
