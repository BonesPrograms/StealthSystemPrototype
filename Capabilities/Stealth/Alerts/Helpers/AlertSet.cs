using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Collections;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.Collections;

using SerializeField = UnityEngine.SerializeField;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.AlertExtensions;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public abstract class AlertSet : CoalescibleSet<IAlert>
    {
        #region Debug
        [UD_DebugRegistry]
        public static void AlertSet_DoDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Alerts.AlertSet),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(Add), false },
                    { nameof(Coalesce), false },
                    { nameof(IndexOf), false },
                    { nameof(RemoveType), false },
                    { nameof(TryGet), false },
                    { nameof(GetCoalesce), false },
                });
        }
        #endregion

        private CoalesceMethod? _DefaultCoalesceMethod;
        public virtual CoalesceMethod DefaultCoalesceMethod { get; }

        private bool IsCoalescing = false;

        #region Constructors

        public AlertSet()
            : base()
        { }
        public AlertSet(int Capacity)
            : base(Capacity)
        { }
        public AlertSet(IReadOnlyList<IAlert> List, CoalesceMethod DefaultCoalesceMethod)
            : base(List.Select(a => a.Copy()).ToList())
        {
            _DefaultCoalesceMethod = DefaultCoalesceMethod;
            Coalesce();
        }
        public AlertSet(CoalesceMethod DefaultCoalesceMethod)
            : base()
        {
            _DefaultCoalesceMethod = DefaultCoalesceMethod;
        }
        public AlertSet(IReadOnlyList<IAlert> List)
            : this(List, CoalesceMethod.Merge)
        {
        }
        public AlertSet(AlertSet Source)
            : this(Source as IReadOnlyList<IAlert>)
        { }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);

            Writer.Write(_DefaultCoalesceMethod != null);
            if (_DefaultCoalesceMethod != null)
                Writer.WriteOptimized((int)_DefaultCoalesceMethod);
        }

        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            if (Reader.ReadBoolean())
                _DefaultCoalesceMethod = (CoalesceMethod)Reader.ReadOptimizedInt32();
        }

        #endregion

        public AlertSet Coalesce(CoalesceMethod? Method = null)
        {
            _DefaultCoalesceMethod ??= CoalesceMethod.Merge;
            CoalesceMethod method = Method ?? DefaultCoalesceMethod;

            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(method), method.ToStringWithNum()),
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            if (!IsCoalescing)
            {
                IsCoalescing.Toggle();

                Debug.Log("Coalescing Alerts...", Indent: indent[1]);
                Dictionary<string, BaseAlert> coalescedList = new();
                for (int i = 0; i < Count; i++)
                {
                    if (Items[i] is BaseAlert currentAlert)
                    {
                        BaseAlert newAlert = currentAlert;
                        if (coalescedList.ContainsKey(currentAlert.Name))
                        {
                            if (coalescedList[currentAlert.Name] is BaseAlert storedAlert)
                                newAlert = storedAlert.Coalesce(newAlert, method);
                        }
                        Debug.Log(currentAlert.ToString() + " -> " + newAlert.ToString(), Indent: indent[2]);
                        coalescedList[currentAlert.Name] = newAlert;
                    }
                }
                Clear();
                Debug.Log("Re-racking Alerts...", Indent: indent[1]);
                if (!coalescedList.Values.IsNullOrEmpty()
                    && new List<BaseAlert>(coalescedList.Values) is var newAlerts)
                {
                    EnsureCapacity(newAlerts.Count);
                    for (Length = 0; Length < newAlerts.Count; Length++)
                    {
                        Items[Length] = newAlerts[Length];
                        Debug.Log(Items[Length].ToString(), Indent: indent[2]);
                    }
                    Variant++;
                }

                IsCoalescing.Toggle();
            }
            return this;
        }

        public override void Add(BaseAlert Alert)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            base.Add(Alert);
            Coalesce();
        }

        public override void Insert(int Index, BaseAlert Item)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            base.Insert(Index, Item);
            Coalesce();
        }

        public bool AnySame<A>(A Alert = null)
            where A : BaseAlert, new()
            => Coalesce().Any(a => a.IsSame(Alert) || a.IsType(typeof(A)));

        public A GetCoalesce<A>(A Alert, CoalesceMethod? Method = null)
            where A : BaseAlert, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            CoalesceMethod method = Method ?? DefaultCoalesceMethod;
            if (Coalesce(method).TryGet(out A existingAlert))
                return Alert.Coalesce(existingAlert, method);
            return Alert;
        }

        public bool TryGet<A>(out A Value)
            where A : BaseAlert, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            Value = null;
            Coalesce();
            for (int i = 0; i < Count; i++)
                if (Items[i].IsType<A>())
                {
                    Value = Items[i] as A;
                    return true;
                }
            return false;
        }

        public bool RemoveType<A>(A Alert = null)
            where A : BaseAlert, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            int itemsCount = Coalesce().Count;
            Items.GetIndices(Where: a => a.IsSame(Alert) || a.IsType<A>()).ToList().ForEach(i => RemoveAt(i));
            return itemsCount != Count;
        }

        public int IndexOf<A>(A Alert = null)
            where A : BaseAlert, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            Coalesce();
            for (int i = 0; i < Count; i++)
                if (Items[i] is BaseAlert storedAlert
                    && storedAlert.IsType<A>())
                    return i;

            return -1;
        }

        public virtual string DebugString(string Header = null)
            => (Header.IsNullOrEmpty() ? null : Header + ":\n") +
                Coalesce().AggregateNewLineDelimited();

        public override void Dispose()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(IsCoalescing), IsCoalescing),
                });

            Clear();
            _DefaultCoalesceMethod = null;
            IsCoalescing = false;
        }
    }
}
