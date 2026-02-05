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
        , IList<T>
        , IReadOnlyList<T>
        where T
        : ICoalescible
        , IComposite
    {
        public T this[int Index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public int IndexOf(T Item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int Index, T Item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int Index)
        {
            throw new NotImplementedException();
        }
    }
}
