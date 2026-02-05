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
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DebuggerDisplay("Count = {Count}")]
    [Serializable]
    public abstract partial class CoalescibleSet<T>
        : IComposite
        , IDisposable
    //  , IEnumerable<T>
    //  , ICollection<IAlert>
    //  , IReadOnlyCollection<IAlert>
    //  , ISet<IAlert>
    //  , IList<IAlert>
    //  , IReadOnlyList<IAlert>
        where T : ICoalescible, IComposite
    {
        #region Debug
        /*
        [UD_DebugRegistry]
        public static void CoalescibleSet_DoDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.CoalescibleSet<T>),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(Add), false },
                });
        }
        */
        #endregion
        #region Instance Fields & Properties

        protected T[] Items = Array.Empty<T>();

        protected int Length;
        protected int Size;
        protected int Variant;

        public int Capacity => Size;
        public virtual int DefaultCapacity => 4;
        public int Version => Variant;

        #endregion
        #region Constructors

        public CoalescibleSet()
        {
            Length = 0;
            Size = 0;
            Variant = 0;
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(Length);
            for (int i = 0; i < Length; i++)
                Writer.Write(Items[i]);
        }

        public virtual void Read(SerializationReader Reader)
        {
            Size = (Length = Reader.ReadOptimizedInt32());
            Items = new T[Size];
            for (int i = 0; i < Length; i++)
                Items[i] = (T)Reader.ReadComposite();
        }

        #endregion

        // resize, ensure capacity, etc.

        #region Disposable

        public virtual void Dispose()
        {
            Items = null;
            Length = 0;
            Size = 0;
            Variant = 0;
        }

        #endregion
    }
}
