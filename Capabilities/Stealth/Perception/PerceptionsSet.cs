using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using XRL;
using XRL.Collections;
using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Perceptions.BasePerception;
using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Const;
using StealthSystemPrototype.Coalescence;
using StealthSystemPrototype.Perceptions.Helpers;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class PerceptionsSet : CoalescibleSet<IPerception>
    {
        #region Debug
        [UD_DebugRegistry]
        public static void PerceptionRack_DoDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Capabilities.Stealth.PerceptionsSet),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(GetPerceptionsBestFirst), false },
                    { nameof(GetAlertContexts), false },
                });
        #endregion
        #region Static & Const

        public static PerceptionEqualityComparer DefaultEqualityComparer => new();
        public static PerceptionCoalescer DefaultCoalescer => new(CoalesceMethod.Greater);

        #endregion

        [NonSerialized]
        protected GameObject _Perceiver = null;
        public GameObject Perceiver
        {
            get => _Perceiver;
            set
            {
                _Perceiver = value;
                for (int i = 0; i < Length; i++)
                    Items[i].Perceiver = _Perceiver;
            }
        }

        #region Constructors

        public PerceptionsSet()
            : base(DefaultEqualityComparer, DefaultCoalescer)
        { }
        public PerceptionsSet(GameObject Perceiver)
            : this()
        {
            this.Perceiver = Perceiver;
        }
        public PerceptionsSet(IReadOnlyList<IPerception> SourceList)
            : base(SourceList, DefaultEqualityComparer, DefaultCoalescer)
        { }
        public PerceptionsSet(GameObject Perceiver, IReadOnlyList<IPerception> SourceList)
            : this(SourceList)
        {
            this.Perceiver = Perceiver;
        }
        public PerceptionsSet(PerceptionsSet Source)
            : this(Source.Perceiver, Source)
        { }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteGameObject(Perceiver);
            Writer.WriteOptimized(Length);
            for (int i = 0; i < Length; i++)
                Writer.Write(Items[i]);
        }

        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            _Perceiver = Reader.ReadGameObject();
            int written = Reader.ReadOptimizedInt32();
            for (int i = 0; i < written; i++)
                Add(Reader.ReadComposite() as IPerception, DuringSerialization: true);
        }

        public void FinalizeRead(SerializationReader Reader)
        {
            if (_Perceiver != null)
                Perceiver = _Perceiver;

            foreach (var perception in this)
                perception.FinalizeRead(Reader);
        }

        #endregion

        private string AggregatePerception(
            string Accumulator,
            IPerception Next,
            string Delimiter,
            bool Short)
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? Delimiter : null) + Next.ToString(Short: Short);

        private string AggregatePerceptionAlert(
            string Accumulator,
            IPerception Next,
            string Delimiter,
            bool Short,
            IAlert Alert = null)
            => Alert == null
                || Next.CanPerceiveAlert(Alert)
            ? AggregatePerception(
                Accumulator: Accumulator,
                Next: Next,
                Delimiter: Delimiter,
                Short: Short)
            : Accumulator;

        public virtual string ToString(
            string Delimiter,
            bool Short,
            IAlert Alert = null)
            => Items?.Aggregate("", (a, n) => AggregatePerceptionAlert(a, n, Delimiter, Short, Alert));

        public virtual string ToString(
            bool Short)
            => ToString(", ", Short, null);

        public virtual string ToStringLines(
            bool Short = false)
            => ToString("\n", Short, null);

        public override string ToString()
            => ToString(Short: false);

        public override bool Add(IPerception Item)
            => Add(Item, false);

        public virtual bool Add(IPerception Item, bool DuringSerialization)
        {
            if (base.Add(Item))
            {
                Item.Perceiver = Perceiver;
                if (!DuringSerialization)
                    Item.Added();

                return true;
            }
            return false;
        }

        public P Add<P>(
            int Level,
            int PurviewValue,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where P : class, IPerception, new()
        {
            P perception = new()
            {
                Level = Level,
            };
            if (perception != null)
            {
                perception.ConfigurePurview(PurviewValue);
                Add(Item: perception);
                return perception;
            }

            return null;
        }

        public P Add<P>(
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where P : class, IPerception, new()
            => Add<P>(0, IPurview.DEFAULT_VALUE, DoRegistration, Initial, Creation);

        public P Add<P>(
            int Level,
            int PurviewValue,
            bool DoRegistration = true,
            bool Creation = false)
            where P : class, IPerception, new()
            => Add<P>(Level, PurviewValue, DoRegistration, false, Creation);

        public bool Has<P>()
            where P : class, IPerception, new()
            => Contains<P>();

        public bool Has(IPerception Perception)
            => base.Contains(Perception);

        public bool HasAlert<A>()
            where A : class, IAlert, new()
            => ContainsAlert<A>();

        protected static bool IsPerceptionName(IPerception Perception, string PerceptionName)
            => Perception.GetName() == PerceptionName;

        protected static bool IsPerceptionShortName(IPerception Perception, string PerceptionShortName)
            => Perception.GetName(Short: true) == PerceptionShortName;

        protected static bool IsPerceptionName(IPerception Perception, string PerceptionName, bool IncludeShort)
            => IsPerceptionName(Perception, PerceptionName)
            || (IncludeShort
                && IsPerceptionShortName(Perception, PerceptionName));

        public bool Has(string PerceptionName, bool IncludeShort = false)
            => AsEnumerable(p => IsPerceptionName(p, PerceptionName, IncludeShort))
            ?.FirstOrDefault() != null;

        public P Get<P>()
            where P : class, IPerception, new()
        {
            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == typeof(P))
                    return Items[i] as P;
            return null;
        }

        public IPerception GetOfType(Type Type)
        {
            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == Type)
                    return Items[i];
            return null;
        }

        public List<IAlertTypedPerception<A>> GetForAlert<A>(A Alert = null)
            where A : class, IAlert, new()
        {
            var output = new List<IAlertTypedPerception<A>>();

            for (int i = 0; i < Count; i++)
                if (Items[i] is IAlertTypedPerception<A> typedPerception)
                    output.Add(typedPerception);

            return output;
        }

        public IPerception Get(string PerceptionName, bool IncludeShort = false)
            => AsEnumerable(p => IsPerceptionName(p, PerceptionName, IncludeShort))
            ?.FirstOrDefault();

        protected static bool IsPerceptionOfAlert<A>(IPerception Perception)
            where A : class, IAlert, new()
            => Perception is IAlertTypedPerception<A>;

        public IAlertTypedPerception<A> GetFirstOfAlert<A>(A Alert)
            where A : class, IAlert, new()
            => AsEnumerable<A>()
                ?.FirstOrDefault();

        public IAlertTypedPerception<A> GetFirstTypedOfAlert<A>(A Alert)
            where A : class, IAlert, new()
            => AsEnumerable<A>()
                ?.FirstOrDefault();

        public bool TryGet<P>(out P Perception)
            where P : class, IPerception, new()
            => (Perception = Get<P>()) != null;

        public bool TryGet(string Name, out IPerception Perception)
            => (Perception = Get(Name)) != null;

        public virtual PerceptionsSet DeepCopy(GameObject Parent)
        {
            var perceptionSet = (PerceptionsSet)Activator.CreateInstance(GetType());

            var fields = GetType().GetFields();

            foreach (var fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perceptionSet, fieldInfo.GetValue(this));

            perceptionSet.Perceiver = Parent;

            perceptionSet.Items = new IPerception[DefaultCapacity];
            perceptionSet.EnsureCapacity(Size);

            for (int i = 0; i < Length; i++)
                perceptionSet.Items[i] = Items[i].DeepCopy(Parent);

            return perceptionSet;
        }

        public bool Validate(bool RemoveInvalid = true)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                });

            bool allValid = true;
            using var removeList = ScopeDisposedList<IPerception>.GetFromPool();
            foreach (var perception in this)
            {
                if (!perception.Validate())
                {
                    Debug.CheckNah(perception?.GetName() ?? "NO_PERCEPTION", "Invalid", Indent: indent[1]);
                    if (RemoveInvalid)
                        removeList.Add(perception);
                    else
                        allValid = false;
                }
            }
            foreach (var perception in removeList)
                Remove(perception);

            removeList.Clear();

            if (RemoveInvalid
                && !allValid)
                allValid = Validate(false);

            return allValid;
        }

        public void ClearCaches()
        {
            for (int i = 0; i < Count; i++)
                Items[i].ClearCaches();
        }

        public IEnumerable<IPerception> GetPerceptionsBestFirst(
            Comparison<IPerception> Comparison,
            Predicate<IPerception> Filter,
            bool ClearFirst)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            if (Items.ToList() is not List<IPerception> perceptionsList)
                return null;

            if (ClearFirst)
                ClearCaches();

            perceptionsList.Sort(Comparison);

            return perceptionsList
                ?.Where(Filter.ToFunc());
        }

        public static int BestForThisAlert<A>(A Alert, IPerception x, IPerception y)
            where A : IAlert, new()
            => new PerceptionComparer(ComparisonType.EffectiveLevel, Alert).Compare(x, y);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(bool ClearFirst, IAlert Alert = null)
            => GetPerceptionsBestFirst(
                Comparison: new PerceptionComparer(ComparisonType.EffectiveLevel, Alert).Compare,
                Filter: p => p.CanPerceiveAlert(Alert),
                ClearFirst: ClearFirst);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(IAlert Alert = null)
            => GetPerceptionsBestFirst(true, Alert);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(AlertContext Context, bool ClearFirst)
            => GetPerceptionsBestFirst(true, Context.ActionAlert);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(AlertContext Context)
            => GetPerceptionsBestFirst(Context, true);

        public IPerception GetHighestRatedPerceptionFor(IAlert Alert)
        {
            using var indent = new Indent(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Perceiver), Perceiver?.DebugName ?? "null"),
                    Debug.Arg(nameof(Alert), Alert?.Name ?? Alert.Type?.ToStringWithGenerics()),
                });

            if (GetPerceptionsBestFirst(Alert) is not IEnumerable<IPerception> highestFirstList
                || highestFirstList.IsNullOrEmpty())
            {
                Debug.CheckNah("Set empty or null", Indent: indent[1]);
                return null;
            }
            var output = highestFirstList.First();
            Debug.CheckYeh("Got", output, Indent: indent[1]);
            return output;
        }

        public virtual void TickCooldowns()
        {
            foreach (IPerception perception in this)
                perception.TickCooldown();
        }
        public void PutAllOnCooldown(int Cooldown)
        {
            foreach (IPerception perception in this)
                perception.GoOnCooldown(Cooldown);
        }
        public void PutAllOnCooldown()
        {
            foreach (IPerception perception in this)
                perception.GoOnCooldown();
        }
        public void TakeAllOffCooldown()
        {
            foreach (IPerception perception in this)
                perception.GoOffCooldown();
        }

        public virtual bool CanPerceiveAlert(IAlert Alert)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Alert?.ToString() ?? "NO_ALERT"),
                });

            if (Alert == null)
                throw new ArgumentNullException(nameof(Alert), nameof(this.CanPerceiveAlert) + " requires an " + nameof(IAlert) + " to check for perceivablitiy.");

            for (int i = 0; i < Count; i++)
                if (Items[i].CanPerceiveAlert(Alert))
                    return true;

            return false;
        }

        public bool CanPerceive(AlertContext Context)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Context?.ActionAlert?.ToString() ?? "NO_CONTEXT"),
                });

            if (Context?.ActionAlert is IAlert alert)
            {
                if (CanPerceiveAlert(alert))
                    return true;

                for (int i = 0; i < Count; i++)
                    if (Items[i].CanPerceive(Context))
                        return true;
            }
            return false;
        }

        public virtual bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Context?.ActionAlert?.ToString() ?? "NO_CONTEXT"),
                });
            SuccessMargin = 0;
            FailureMargin = 0;

            bool any = false;
            foreach (IPerception perception in GetPerceptionsBestFirst(Context) ?? new IPerception[0])
            {
                any = perception.TryPerceive(Context, out int successMargin, out int failureMargin)
                    || any;

                GetMinMax(out _, out SuccessMargin, SuccessMargin, successMargin);
                GetMinMax(out FailureMargin, out _, FailureMargin, failureMargin);
            }

            if (any)
                FailureMargin = 0;
            else
                SuccessMargin = 0;

            return any;
        }

        public virtual bool TryPerceive(BaseConcealedAction ConcealedAction)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "NO_OWNER"),
                    Debug.Arg(nameof(ConcealedAction.Sneaker), ConcealedAction.Sneaker?.DebugName ?? "NO_HIDER"),
                });

            bool any = false;
            if (GetAlertContexts(ConcealedAction) is IEnumerable<AlertContext> alertContexts)
            {
                Debug.CheckYeh(nameof(alertContexts), alertContexts.Count(), Indent: indent[1]);
                foreach (AlertContext context in alertContexts)
                {
                    bool didPercieve = TryPerceive(context, out int SuccessMargin, out int FailureMargin);
                    any = didPercieve || any;
                    Debug.YehNah(context.ActionAlert.ToString() ?? "NO_CONTEXT", didPercieve, Indent: indent[2]);

                    if (didPercieve)
                        context.RaiseDetection();
                }
            }
            else
                Debug.CheckNah(nameof(alertContexts), "null", Indent: indent[1]);
            return any;
        }
            //=> GetAlertContexts(ConcealedAction).Aggregate(false, (a, n) => TryPerceive(n) || a);

        #region Container Helpers

        public virtual bool ContainsType(Type Type)
        {
            if (Type == null)
                throw new ArgumentNullException(nameof(Type));

            for (int i = 0; i < Length; i++)
                if (Items[i].GetType() == Type)
                    return true;

            return false;
        }

        public virtual bool ContainsAlert<A>()
            where A : class, IAlert, new()
        {
            for (int i = 0; i < Length; i++)
                if (Items[i] is IAlertTypedPerception<A>)
                    return true;

            return false;
        }
        public virtual bool ContainsType(IPerception Perception)
            => ContainsType(Perception.GetType());

        public virtual bool Contains<P>(P Perception = null)
            where P : class, IPerception, new()
            => ContainsType(Perception?.GetType() ?? typeof(P));

        #endregion
        #region Conversion Methods

        public IEnumerable<IPerception> AsEnumerable(Predicate<IPerception> Filter = null)
        {
            try
            {
                if (Items == null)
                    throw new InnerArrayNullException(nameof(Items));

                return Items.Where(Filter?.ToFunc());
            }
            catch (InnerArrayNullException)
            {
                return new IPerception[0];
            }
        }

        protected static bool IsAlertTypedPerception<A>(IPerception Perception)
            where A : class, IAlert, new()
            => Perception is IAlertTypedPerception<A>;

        protected static IAlertTypedPerception<A> AsAlertTypedPerception<A>(IPerception Perception)
            where A : class, IAlert, new()
            => Perception as IAlertTypedPerception<A>;

        public IEnumerable<IAlertTypedPerception<A>> AsEnumerable<A>(Predicate<IAlertTypedPerception<A>> Filter = null)
            where A : class, IAlert, new()
        {
            try
            {
                if (Items == null)
                    throw new InnerArrayNullException(nameof(Items));

                return Items
                    .Where(IsAlertTypedPerception<A>)
                    .Select(AsAlertTypedPerception<A>)
                    .Where(Filter.ToFunc());
            }
            catch (InnerArrayNullException)
            {
                return new IAlertTypedPerception<A>[0];
            }
        }

        public IEnumerable<AlertContext> GetAlertContexts(BaseConcealedAction ConcealedAction)
        {
            if (ConcealedAction == null)
                throw new ArgumentNullException(
                    paramName: nameof(ConcealedAction),
                    message: nameof(this.GetAlertContexts) + " requires a " + nameof(BaseConcealedAction) + " from which to construct " + nameof(AlertContext).Pluralize() + ".");

            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "NO_OWNER"),
                    Debug.Arg(nameof(ConcealedAction.Sneaker), ConcealedAction.Sneaker?.DebugName ?? "NO_HIDER"),
                });

            GameObject alertObject = ConcealedAction.AlertObject ?? ConcealedAction.Sneaker;
            Cell alertLocation = ConcealedAction.AlertLocation ?? ConcealedAction.Sneaker?.CurrentCell;

            // iterate all the alerts in the concealed action.
            // these represent how obvious the action was to this type of sense.
            foreach (BaseAlert actionAlert in ConcealedAction)
                yield return new AlertContext(
                    ParentAction: ConcealedAction,
                    Perceiver: Perceiver,
                    ActionAlert: actionAlert.DeepCopy(Degrade: false),
                    SneakAlert: ConcealedAction.SneakPerformance[actionAlert],
                    Hider: ConcealedAction.Sneaker,
                    AlertObject: alertObject,
                    AlertLocation: alertLocation);
        }

        #endregion
    }
}
