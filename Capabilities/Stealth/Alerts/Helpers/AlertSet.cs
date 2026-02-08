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
using StealthSystemPrototype.Coalescence;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.AlertExtensions;
using static StealthSystemPrototype.Coalescer<StealthSystemPrototype.Alerts.IAlert>;
using static StealthSystemPrototype.Alerts.AlertEqualityComparer;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public class AlertSet : CoalescibleSet<IAlert>
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
                    { nameof(IndexOf), false },
                    { nameof(TryGet), false },
                });
        }
        #endregion

        public static AlertEqualityComparer DefaultEqualityComparer => new(DefaultEqualityComparisonType);
        public static EqualityComparisonType DefaultEqualityComparisonType => EqualityComparisonType.Type;
        public static AlertCoalescer DefaultCoalescer => new(DefaultCoalescMethod);
        public static CoalesceMethod DefaultCoalescMethod => CoalesceMethod.Combine;

        #region Constructors

        public AlertSet()
            : base()
        {
            EqualityComparer = DefaultEqualityComparer;
            Coalescer = DefaultCoalescer;
        }
        public AlertSet(int Capacity, AlertEqualityComparer EqualityComparer, AlertCoalescer Coalescer)
            : base(
                  Capacity: Capacity,
                  EqualityComparer: EqualityComparer ?? DefaultEqualityComparer,
                  Coalescer: Coalescer ?? DefaultCoalescer)
        { }
        public AlertSet(int Capacity, EqualityComparisonType ComparisonType, CoalesceMethod CoalesceMethod)
            : base(
                  Capacity: Capacity,
                  EqualityComparer: new AlertEqualityComparer(ComparisonType),
                  Coalescer: new AlertCoalescer(CoalesceMethod))
        { }
        public AlertSet(IReadOnlyList<IAlert> List, AlertEqualityComparer EqualityComparer, AlertCoalescer Coalescer)
            : base(
                  List: List.Select(a => a.Copy()).ToList(),
                  EqualityComparer: EqualityComparer ?? DefaultEqualityComparer,
                  Coalescer: Coalescer ?? DefaultCoalescer)
        { }
        public AlertSet(IReadOnlyList<IAlert> List, CoalesceMethod CoalesceMethod)
            : this(
                  List: List.Select(a => a.Copy()).ToList(),
                  EqualityComparer: null,
                  Coalescer: new AlertCoalescer(CoalesceMethod))
        { }
        public AlertSet(CoalesceMethod CoalesceMethod)
            : base(0, DefaultEqualityComparer, new AlertCoalescer(CoalesceMethod))
        { }
        public AlertSet(IReadOnlyList<IAlert> List)
            : this(List, null, null)
        { }
        public AlertSet(AlertSet Source)
            : this(
                  List: Source as IReadOnlyList<IAlert>,
                  EqualityComparer: Source.EqualityComparer as AlertEqualityComparer,
                  Coalescer: Source.Coalescer as AlertCoalescer)
        { }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // do writing here
        }

        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // do reading here
        }

        #endregion

        public int IndexOf<A>()
            where A : IAlert
            => Items
                .Select((o, i) => new KeyValuePair<int, IAlert>(i, o)) // convert to IEnumerable of KVP<int, IAlert> where int is Index
                .Aggregate(-1, (a, n) // start with -1 (no item)
                    => n.Value.IsType(typeof(A)) // if (n)ext.Value matches type of A
                        && a < 0 // but only the first one (should typically only be 1)
                    ? n.Key // (a)ccumulator = n.Key (the Index)
                    : a); // otherwise a is unchanged.

        public bool TryGetIndexOf<A>(out int Index)
            where A : IAlert
            => (Index = IndexOf<A>()) >= 0;

        public bool TryGet<A>(out A Value)
            where A : IAlert
        {
            Value = default;
            if (!TryGetIndexOf<A>(out int index))
                return false;

            Value = (A)Items[index];
            return true;
        }

        public virtual string DebugString(string Header = null)
            => (Header.IsNullOrEmpty() ? null : Header + ":\n") +
                this.CoalesceNewLineDelimited();

        public override void Dispose()
        {
            base.Dispose();
            // do disposing here
        }
    }
}
