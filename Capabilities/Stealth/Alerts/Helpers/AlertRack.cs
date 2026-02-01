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
    public class AlertRack : Rack<BaseAlert>, IDisposable
    {
        private CoalesceMethod? _DefaultCoalesceMethod;
        public virtual CoalesceMethod DefaultCoalesceMethod { get; }

        #region Constructors

        public AlertRack()
            : base()
        { }
        public AlertRack(int Capacity)
            : base(Capacity)
        { }
        public AlertRack(IReadOnlyList<BaseAlert> List, CoalesceMethod DefaultCoalesceMethod)
            : base(List.Select(a => a.Copy()).ToList())
        {
            _DefaultCoalesceMethod = DefaultCoalesceMethod;
            Coalesce();
        }
        public AlertRack(CoalesceMethod DefaultCoalesceMethod)
            : base()
        {
            _DefaultCoalesceMethod = DefaultCoalesceMethod;
        }
        public AlertRack(IReadOnlyList<BaseAlert> List)
            : this(List, CoalesceMethod.Merge)
        {
        }
        public AlertRack(AlertRack Source)
            : this(Source as IReadOnlyList<BaseAlert>)
        { }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            Coalesce();

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

        public virtual AlertRack Coalesce(CoalesceMethod? Method = null)
        {
            _DefaultCoalesceMethod ??= CoalesceMethod.Merge;

            CoalesceMethod method = Method ?? DefaultCoalesceMethod;
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
                    coalescedList[currentAlert.Name] = newAlert;
                }
            }
            Clear();
            AddRange(coalescedList.Values);
            Variant++;
            return this;
        }

        public override void Add(BaseAlert Alert)
        {
            List<int> indicesToRemove = new();
            for (int i = 0; i < Coalesce().Count; i++)
            {
                if (Items[i].IsSame(Alert))
                {
                    Alert = Alert.Coalesce(Items[i], Method: DefaultCoalesceMethod);
                    indicesToRemove.Add(i);
                }
            }
            foreach (int index in indicesToRemove)
                RemoveAt(index);

            base.Add(Alert);
        }

        public override void Insert(int Index, BaseAlert Item)
        {
            base.Insert(Index, Item);
            Coalesce();
        }

        public bool AnySame<A>(A Alert = null)
            where A : BaseAlert, new()
            => Coalesce().Any(a => a.IsSame(Alert) || a.IsType(typeof(A)));

        public A GetCoalesce<A>(A Alert, CoalesceMethod? Method = null)
            where A : BaseAlert, new()
        {
            CoalesceMethod method = Method ?? DefaultCoalesceMethod;
            if (Coalesce(method).TryGet(out A existingAlert))
                return Alert.Coalesce(existingAlert, method);
            return Alert;
        }

        public bool TryGet<A>(out A Value)
            where A : BaseAlert, new()
        {
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
            int itemsCount = Coalesce().Count;
            Items.GetIndices(Where: a => a.IsSame(Alert) || a.IsType<A>()).ToList().ForEach(i => RemoveAt(i));
            return itemsCount != Count;
        }

        public int IndexOf<A>(A Alert = null)
            where A : BaseAlert, new()
        {
            Coalesce();
            for (int i = 0; i < Count; i++)
                if (Items[i] is BaseAlert storedAlert
                    && storedAlert.IsType<A>())
                    return i;

            return -1;
        }

        public new Enumerator GetEnumerator()
            => ((Container<BaseAlert>)Coalesce()).GetEnumerator();

        public virtual string DebugString(string Header = null)
            => (Header.IsNullOrEmpty() ? null : Header + ":\n") +
                Coalesce().AggregateNewLineDelimited();

        public virtual void Dispose()
        {
            Clear();
            _DefaultCoalesceMethod = null;
        }
    }
}
