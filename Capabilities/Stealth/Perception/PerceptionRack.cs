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

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public partial class PerceptionRack : Rack<IPerception>
    {
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Capabilities.Stealth.PerceptionRack),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(Add), false },
                    { nameof(HasWantEvent), false },
                    { nameof(PerceptionWantsEvent), false },
                });
        #endregion

        private GameObject _Owner;

        public GameObject Owner
        {
            get => _Owner;
            set
            {
                _Owner = value;
                for (int i = 0; i < Length; i++)
                    Items[i].Owner = _Owner;
            }
        }

        #region Constructors

        public PerceptionRack()
            : base()
        {
            _Owner = null;
        }
        public PerceptionRack(int Capacity)
            : base(Capacity)
        {
            _Owner = null;
        }
        public PerceptionRack(GameObject Owner)
            : this()
        {
            _Owner = Owner;
        }
        public PerceptionRack(GameObject Owner, int Capacity)
            : this(Capacity)
        {
            _Owner = Owner;
        }
        public PerceptionRack(IReadOnlyCollection<IPerception> Source)
            : this(Source.Count)
        {
            Length = Source.Count;
            Items = Source.ToArray();
        }
        public PerceptionRack(GameObject Owner, IReadOnlyCollection<IPerception> Source)
            : this(Source)
        {
            this.Owner = Owner;
        }
        public PerceptionRack(PerceptionRack Source)
            : this(Source.Owner, (IReadOnlyCollection<IPerception>)Source)
        {
        }
        public PerceptionRack(GameObject NewOwner, PerceptionRack Source)
            : this(Source)
        {
            Owner = NewOwner;
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteOptimized(Variant);
            Writer.WriteGameObject(Owner);
        }

        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Variant = Reader.ReadOptimizedInt32();
            _Owner = Reader.ReadGameObject();
        }

        public void FinalizeRead(GameObject Basis, SerializationReader Reader)
        {
            int i = 0;
            for (int length = Length; i < length; i++)
            {
                IPerception perception = Items[i];
                perception.ApplyRegistrar(Basis);
                perception.FinalizeRead(Reader);
                if (length != Length)
                {
                    length = Length;
                    if (i < length
                        && Items[i] != perception)
                        i--;
                }
            }
            Owner = _Owner;
        }

        #endregion

        private string AggregatePerception(
            string Accumulator,
            IPerception Next,
            string Delimiter,
            bool Short,
            GameObject Entity)
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? Delimiter : null) + Next.ToString(Short: Short);

        private string AggregatePerceptionAlert(
            string Accumulator,
            IPerception Next,
            string Delimiter,
            bool Short,
            GameObject Entity,
            IAlert Alert = null)
            => Alert == null
                || Next.CanPerceiveAlert(Alert)
            ? AggregatePerception(
                Accumulator: Accumulator,
                Next: Next,
                Delimiter: Delimiter,
                Short: Short,
                Entity: Entity)
            : Accumulator;

        public virtual string ToString(
            string Delimiter,
            bool Short,
            GameObject Entity,
            IAlert Alert = null)
        {
            if (Items == null)
                MetricsManager.LogException(
                    Context: CallChain(nameof(PerceptionRack), nameof(ToString)),
                    x: new InnerArrayNullException(nameof(Items)),
                    category: GAME_MOD_EXCEPTION);

            IPerception[] items = new IPerception[Count];
            Array.Copy(Items, items, Count);
            return items?.Aggregate("", (a, n) => AggregatePerceptionAlert(a, n, Delimiter, Short, Entity, Alert));
        }

        public virtual string ToString(
            bool Short,
            GameObject Entity = null)
            => ToString(", ", Short, Entity, null);

        public virtual string ToStringLines(
            bool Short = false,
            GameObject Entity = null)
            => ToString("\n", Short, Entity, null);

        public override string ToString()
            => ToString(Short: false, Entity: null);

        public sealed override void Add(IPerception Item)
            => Add(Item);

        public void Add<P>(
            P Perception,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where P : IPerception
        {
            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            if (Perception == null)
                return;

            if (Perception.Owner != Owner)
                Perception.Owner = Owner;

            Owner?.FlushTransientCache();

            base.Add(Perception);
            
            if (DoRegistration)
            {
                Perception.Owner = Owner;
                Perception.ApplyRegistrar(Owner);
            }
            if (Initial)
            {
                Perception.Initialize();
            }
            Perception.Attach();
            if (!Creation)
            {
                Perception.AddedAfterCreation();
            }
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
                perception.AssignDefaultPurview(PurviewValue);
                // perception.Purview.SetParentPerception(perception);
                Add(
                    Perception: perception,
                    DoRegistration: DoRegistration,
                    Initial: Initial,
                    Creation: Creation);
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
            where A : class, IAlert<A>, new()
            => ContainsAlert<A>();

        protected static bool IsPerceptionName(IPerception Perception, string PerceptionName)
            => Perception.Name == PerceptionName;


        protected static bool IsPerceptionShortName(IPerception Perception, string PerceptionShortName)
            => Perception.ShortName == PerceptionShortName;

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

        public List<IAlertTypedPerception<A, IPurview<A>>> GetForAlert<A>(A Alert = null)
            where A : class, IAlert, new()
        {
            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            List<IAlertTypedPerception<A, IPurview<A>>> output = new();
            for (int i = 0; i < Count; i++)
                if (Items[i] is IAlertTypedPerception<A, IPurview<A>> typedPerception)
                    output.Add(typedPerception);

            return output;
        }

        public IPerception Get(string PerceptionName, bool IncludeShort = false)
            => AsEnumerable(p => IsPerceptionName(p, PerceptionName, IncludeShort))
            ?.FirstOrDefault();

        protected static bool IsPerceptionOfAlert<A>(IPerception IPerception)
            where A : class, IAlert, new()
            => IPerception is IAlertTypedPerception<A, IPurview<A>>;

        public IPerception GetFirstOfAlert<A>(A Alert)
            where A : class, IAlert, new()
            => AsEnumerable<A>()
                ?.FirstOrDefault();

        public IAlertTypedPerception<A, IPurview<A>> GetFirstTypedOfAlert<A>(A Alert)
            where A : class, IAlert, new()
            => AsEnumerable<A>()
                ?.FirstOrDefault();

        public bool TryGet<P>(out P Perception)
            where P : class, IPerception, new()
            => (Perception = Get<P>()) != null;

        public bool TryGet(string Name, out IPerception Perception)
            => (Perception = Get(Name)) != null;

        public P Require<P>(
            bool Creation = false)
            where P : class, IPerception, new()
        {
            if (TryGet(out P perception))
                return perception;

            return Add<P>(DoRegistration: true, Creation);
        }

        protected IPerception RemovePerceptionAt(int Index)
        {
            IPerception perception = TakeAt(Index);
            perception.ApplyUnregistrar(Owner);

            perception.Remove();
            perception.Owner = null;
            Owner.FlushTransientCache();
            return perception;
        }

        public bool Remove<P>(P Perception)
            where P : class, IPerception, new()
        {
            if (Perception == null)
                throw new ArgumentNullException(nameof(Perception), "Cannot be null.");

            if (GetArray() is IPerception[] perceptions)
                for (int i = 0; i < perceptions.Length; i++)
                    if (perceptions[i] == Perception)
                    {
                        RemovePerceptionAt(i);
                        return true;
                    }

            return false;
        }

        public virtual PerceptionRack DeepCopy(GameObject Parent)
        {
            PerceptionRack perceptionRack = (PerceptionRack)Activator.CreateInstance(GetType());

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perceptionRack, fieldInfo.GetValue(this));

            perceptionRack.Owner = Parent;

            perceptionRack.Items = new IPerception[DefaultCapacity];
            perceptionRack.EnsureCapacity(Size);

            for (int i = 0; i < Length; i++)
                perceptionRack.Items[i] = Items[i].DeepCopy(Parent);

            return perceptionRack;
        }

        public bool Validate(bool RemoveInvalid = true)
        {
            Owner = _Owner;
            bool allValid = true;
            List<int> removeList = new();
            for (int i = 0; i < Count; i++)
            {
                if (Items[i] is not IPerception perception)
                    throw new InvalidOperationException(nameof(Items) + " contains null entry at " + i + " despite length of " + Count + ".");

                if (!perception.Validate())
                {
                    if (RemoveInvalid)
                        removeList.Add(i);
                    else
                        allValid = false;
                }
            }
            for (int i = removeList.Count -1; i >= 0; i--)
                RemovePerceptionAt(i);

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
            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            if (Items.ToList() is not List<IPerception> perceptionsList)
                return null;

            if (ClearFirst)
                ClearCaches();

            perceptionsList.Sort(Comparison);

            return perceptionsList
                ?.Where(Filter.ToFunc());
        }
        public IEnumerable<IPerception> GetPerceptionsBestFirst<A>(bool ClearFirst, A Alert = null)
            where A : class, IAlert, new()
            => GetPerceptionsBestFirst(
                Comparison: delegate (IPerception x, IPerception y)
                {
                    Alert ??= new A();
                    int canPerceiveComp = x.CanPerceiveAlert(Alert).CompareTo(y.CanPerceiveAlert(Alert));
                    if (canPerceiveComp != 0)
                        return canPerceiveComp;
                    return x.CompareTo(y);
                },
                Filter: p => p.CanPerceiveAlert(Alert ??= new A()),
                ClearFirst: ClearFirst);

        public IEnumerable<IPerception> GetPerceptionsBestFirst<A>(A Alert = null)
            where A : class, IAlert, new()
            => GetPerceptionsBestFirst(true, Alert);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(AlertContext Context, bool ClearFirst)
            => GetPerceptionsBestFirst(
                Comparison: delegate (IPerception x, IPerception y)
                {
                    int canPerceiveComp = x.CanPerceive(Context).CompareTo(y.CanPerceive(Context));
                    if (canPerceiveComp != 0)
                        return canPerceiveComp;
                    return x.CompareTo(y);
                },
                Filter: p => p.CanPerceive(Context),
                ClearFirst: ClearFirst);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(AlertContext Context)
            => GetPerceptionsBestFirst(Context, true);

        public IPerception GetHighestRatedPerceptionFor<A>(A Alert = null)
            where A : class, IAlert, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Alert), Alert.Name ?? typeof(A)?.ToStringWithGenerics()),
                });

            if (GetPerceptionsBestFirst(Alert) is not List<IPerception> highestFirstList
                || highestFirstList.Count < 1)
            {
                Debug.CheckNah("Entity null, or Rack empty or null", Indent: indent[1]);
                return null;
            }
            IPerception output = highestFirstList[0];
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
            if (Alert == null)
                throw new ArgumentNullException(nameof(Alert), nameof(this.CanPerceiveAlert) + " requires an " + nameof(IAlert) + " to check for perceivablitiy.");

            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            for (int i = 0; i < Count; i++)
                if (Items[i].CanPerceiveAlert(Alert))
                    return true;

            return false;
        }

        public bool CanPerceive(AlertContext Context)
        {
            if (Context?.Alert is IAlert alert)
            {
                if (CanPerceiveAlert(alert))
                    return true;

                if (Items == null)
                    throw new InnerArrayNullException(nameof(Items));

                for (int i = 0; i < Count; i++)
                    if (Items[i].CanPerceive(Context))
                        return true;
            }
            return false;
        }

        public virtual bool TryPerceive(AlertContext Context)
        {
            bool any = false;
            List<IPerception> highestFirst = GetPerceptionsBestFirst(Context)?.ToList() ?? new();
            foreach (IPerception perception in highestFirst)
                any = perception.TryPerceive(Context, out _, out _) || any;

            return any;
        }

        public virtual bool TryPerceive(IConcealedAction ConcealedAction)
            => GetAlertContexts(ConcealedAction).Aggregate(false, (a, n) => TryPerceive(n) || a);

        #region Event Dispatch

        #region Registration

        public void RegisterAll(IEventRegistrar Registrar, IEventSource Source, int EventID, int Order = 0, bool Serialize = false)
        {
            for (int i = 0; i < Length; i++)
                Registrar.Register(Source, Items[i], EventID, Order, Serialize);
        }
        public void RegisterAll(IEventRegistrar Registrar, int EventID, int Order = 0, bool Serialize = false)
            => RegisterAll(Registrar, Owner, EventID, Order, Serialize);

        public void RegisterEvent(IEventSource Source, int EventID, int Order = 0, bool Serialize = false)
        {
            for (int i = 0; i < Length; i++)
                Source.RegisterEvent(Items[i], EventID, Order, Serialize);
        }
        public void RegisterEvent(int EventID, int Order = 0, bool Serialize = false)
            => RegisterEvent(Owner, EventID, Order, Serialize);

        public void UnregisterEvent(IEventSource Source, int EventID)
        {
            for (int i = 0; i < Length; i++)
                Source.UnregisterEvent(Items[i], EventID);
        }
        public void UnregisterEvent(int EventID)
            => UnregisterEvent(Owner, EventID);

        #endregion
        #region MinEvents

        protected static bool GameObjectHasRegisteredEventFrom(GameObject Owner, int ID, IPerception Perception)
            => Owner != null
                && Owner.HasRegisteredEventFrom(ID, Perception);

        public static bool PerceptionWantsEvent(
            int ID,
            int Cascade,
            IPerception Perception,
            GameObject Owner)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perception?.Name ?? "null"),
                    Debug.Arg(MinEvent.EventTypes[ID].ToStringWithGenerics()),
                });

            return Perception.WantEvent(ID, Cascade)
                || GameObjectHasRegisteredEventFrom(Perception.Owner, ID, Perception)
                || GameObjectHasRegisteredEventFrom(Owner, ID, Perception);
        }
        public bool HasWantEvent(int ID, int Cascade)
        {
            if (Items.IsNullOrEmpty())
                return false;

            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(MinEvent.EventTypes[ID].ToStringWithGenerics()),
                    Debug.Arg(nameof(Cascade), Cascade),
                });

            if (MinEvent.CascadeTo(Cascade, MinEvent.CASCADE_NONE))
                return false;

            if (Owner?.RegisteredEvents?.ContainsKey(ID)
                ?? false)
                return true;

            foreach (IPerception perception in this)
                if (PerceptionWantsEvent(ID, Cascade, perception, Owner))
                    return true;

            return false;
        }
        public bool DelegateHandleEvent(MinEvent E)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(MinEvent), E?.TypeStringWithGenerics()),
                });

            if (E.CascadeTo(MinEvent.CASCADE_NONE))
                return true;

            foreach (IPerception perception in this)
            {
                if (!perception.WantEvent(E.ID, E.GetCascadeLevel()))
                    continue;

                if (!E.Dispatch(perception))
                    return false;

                if (!perception.HandleEvent(E))
                    return false;
            }

            return true;
        }

        #endregion

        public bool FireEvent(Event E)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(CallChain(nameof(Event), nameof(Event.ID)), E?.ID),
                });

            if (Owner == null
                || !Owner.HasRegisteredEvent(E.ID))
                return true;

            foreach (IPerception perception in this)
                if (perception.FireEvent(E))
                {
                    Debug.CheckNah(perception.Name, Indent: indent[1]);
                    return false;
                }
            Debug.CheckYeh("All Cleared", Indent: indent[1]);
            return true;
        }

        #endregion
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
            where A : class, IAlert<A>, new()
        {
            for (int i = 0; i < Length; i++)
                if (Items[i] is IAlertTypedPerception<A,IPurview<A>>)
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
            => Perception is IAlertTypedPerception<A, IPurview<A>>;

        protected static IAlertTypedPerception<A, IPurview<A>> AsAlertTypedPerception<A>(IPerception Perception)
            where A : class, IAlert, new()
            => Perception as IAlertTypedPerception<A, IPurview<A>>;

        public IEnumerable<IAlertTypedPerception<A, IPurview<A>>> AsEnumerable<A>(Predicate<IAlertTypedPerception<A, IPurview<A>>> Filter = null)
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
                return new IAlertTypedPerception<A, IPurview<A>>[0];
            }
        }

        public IEnumerable<AlertContext> GetAlertContexts(IConcealedAction ConcealedAction)
        {
            if (ConcealedAction == null)
                throw new ArgumentNullException(
                    paramName: nameof(ConcealedAction),
                    message: nameof(this.GetAlertContexts) + " requires an " + nameof(IConcealedAction) + " from which to construct " + nameof(AlertContext).Pluralize() + ".");

            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            foreach (IAlert alert in ConcealedAction)
                for (int i = 0; i < Count; i++)
                    if (Items[i] is IAlertTypedPerception typedPerception
                        && alert.IsType(typedPerception.AlertType))
                    yield return new AlertContext(
                        ParentAction: ConcealedAction,
                        Perception: typedPerception,
                        Alert: alert,
                        AlertConcealment: ConcealedAction.SneakPerformance[alert],
                        Hider: ConcealedAction.Actor,
                        AlertObject: ConcealedAction.AlertObject ?? ConcealedAction.Actor,
                        AlertLocation: ConcealedAction.AlertLocation ?? ConcealedAction.Actor?.CurrentCell);
        }

        #endregion
    }
}
