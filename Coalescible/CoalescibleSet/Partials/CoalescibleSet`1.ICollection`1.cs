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
        , ICollection<T>
        , IReadOnlyCollection<T>
    //  , ISet<T>
    //  , IList<T>
    //  , IReadOnlyList<T>
        where T
        : ICoalescible
        , IComposite
    {
        public int Count => Length;

        public virtual bool IsReadOnly => false;

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(T Item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] Array, int ArrayIndex)
        {
            throw new NotImplementedException();
        }

        public bool Remove(T Item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.Add(T Item)
        {
            throw new NotImplementedException();
        }
    }
}
