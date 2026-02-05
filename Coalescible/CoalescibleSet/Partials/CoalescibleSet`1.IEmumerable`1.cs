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
        , IEnumerable<T>
    //  , ICollection<IAlert>
    //  , IReadOnlyCollection<IAlert>
    //  , ISet<IAlert>
    //  , IList<IAlert>
    //  , IReadOnlyList<IAlert>
        where T
        : ICoalescible
        , IComposite
    {
        [Serializable]
        public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
        {
            public static InvalidOperationException CollectionModifiedException => new("Collection was modified. Enmumation can't continue.");

            private CoalescibleSet<T> CoalescibleSet;
            private T[] Items;
            private int Index;
            private int Variant;

            public readonly T Current => Items[Index];

            readonly object IEnumerator.Current => Current;

            public Enumerator(CoalescibleSet<T> CoalescibleSet)
            {
                this.CoalescibleSet = CoalescibleSet;
                Items = CoalescibleSet.Items;
                Index = -1;
                Variant = CoalescibleSet.Variant;
            }

            public bool MoveNext()
                => Variant != CoalescibleSet.Variant
                ? throw CollectionModifiedException
                : ++Index < Items.Length;

            public void Reset()
            {
                if (Variant != CoalescibleSet.Variant)
                    throw CollectionModifiedException;

                Index = -1;
            }

            public void Dispose()
            {
                CoalescibleSet = null;
                Items = null;
                Index = default;
                Variant = default;
            }
        }

        public IEnumerator<T> GetEnumerator()
            => new Enumerator(this);

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
