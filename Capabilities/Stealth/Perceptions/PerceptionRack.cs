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
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Const;
using static StealthSystemPrototype.Perceptions.IPerception;
using StealthSystemPrototype.Senses;

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
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? Delimiter : null) + Next.ToString(Short: Short, Entity: Entity);

        private string AggregatePerceptionSense(
            string Accumulator,
            IPerception Next,
            string Delimiter,
            bool Short,
            GameObject Entity,
            ISense Sense = null)
            => Sense == null
                || Next.IsForSense(Sense)
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
            ISense Sense = null)
        {
            if (Items == null)
                MetricsManager.LogException(
                    Context: CallChain(nameof(PerceptionRack), nameof(ToString)),
                    x: new InnerArrayNullException(nameof(Items)),
                    category: GAME_MOD_EXCEPTION);

            IPerception[] items = new IPerception[Count];
            Array.Copy(Items, items, Count);
            return items?.Aggregate("", (a, n) => AggregatePerceptionSense(a, n, Delimiter, Short, Entity, Sense));
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

        public void Add<T>(
            T Perception,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where T : IPerception
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

        public T Add<T, TSense>(
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            T perception = new ();
            Add(perception, DoRegistration, Initial, Creation);
            return perception;
        }

        public T Add<T, TSense>(
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
            => Add<T, TSense>(DoRegistration, false, Creation);

        public bool Has<T, TSense>()
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
            => Contains<T>();

        public bool Has<TSense>(IPerception<TSense> Perception)
            where TSense : ISense<TSense>, new()
            => Contains(Perception);

        public bool HasSense<T>()
            where T : ISense<T>, new()
            => Contains<T>();

        public bool Has(string Name)
            => AsEnumerable(
                p => Name.EqualsAny(new string[]
                {
                    p.Name,
                    p.ShortName,
                    p.GetType().Name,
                    p.GetType().ToString(),
                }))
            ?.FirstOrDefault() != null;

        public IPerception<TSense> Get<T, TSense>()
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == typeof(T))
                    return Items[i] as IPerception<TSense>;
            return null;
        }

        protected IPerception<TSense> Get<TSense>()
            where TSense : ISense<TSense>, new()
            => Get<IPerception<TSense>, TSense>();

        public IPerception Get(string Name)
            => AsEnumerable(
                p => Name.EqualsAny(new string[]
                {
                    p.Name,
                    p.ShortName,
                    p.GetType().Name,
                    p.GetType().ToString(),
                }))
            ?.FirstOrDefault();

        private static bool IsPerceptionOfSense<TSense>(IPerception IPerception)
            where TSense : ISense<TSense>, new()
            => IPerception is IPerception<TSense> perception
            && perception.Sense == typeof(TSense);

        public IPerception<TSense> GetFirstOfSense<TSense>()
            where TSense : ISense<TSense>, new()
            => AsEnumerable(IsPerceptionOfSense<TSense>)
                ?.FirstOrDefault() as IPerception<TSense>;

        public bool TryGet<TSense>(out IPerception<TSense> Perception)
            where TSense : ISense<TSense>, new()
            => (Perception = Get<TSense>()) != null;

        public bool TryGet(string Name, out IPerception Perception)
            => (Perception = Get(Name)) != null;

        public IPerception<TSense> Require<T, TSense>(
            bool Creation = false)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            if (TryGet(out IPerception<TSense> perception))
                return perception;

            return Add<T, TSense>(DoRegistration: true, Creation);
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

        public bool Remove<T>(T Item)
            where T : IPerception, new()
        {
            if (Item == null)
                throw new ArgumentNullException(nameof(Item), "Cannot be null.");

            if (GetArray() is IPerception[] perceptions)
                for (int i = 0; i < perceptions.Length; i++)
                    if (perceptions[i] == Item)
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

        public void ClearRatings()
        {
            for (int i = 0; i < Count; i++)
                Items[i].ClearRating();
        }

        public IEnumerable<IPerception> GetPerceptionsBestFirst(
            GameObject Entity,
            Predicate<IPerception> Filter,
            bool ClearFirst)
        {
            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            if (Items.ToList() is not List<IPerception> perceptionsList)
                return null;

            if (ClearFirst)
                ClearRatings();

            perceptionsList.Sort(new RatingComparer(Entity));

            return perceptionsList
                ?.Where(Filter.ToFunc());
        }
        public IEnumerable<IPerception> GetPerceptionsBestFirst(
            GameObject Entity,
            Predicate<IPerception> Filter)
            => GetPerceptionsBestFirst(Entity, Filter, true);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(
            GameObject Entity,
            bool ClearFirst)
            => GetPerceptionsBestFirst(Entity, null, ClearFirst);

        public IEnumerable<IPerception> GetPerceptionsBestFirst(
            GameObject Entity)
            => GetPerceptionsBestFirst(Entity, null);

        public IPerception GetHighestRatedPerceptionFor(GameObject Entity, bool ClearFirst)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            if (Entity == null
                || GetPerceptionsBestFirst(Entity, ClearFirst) is not List<IPerception> highestFirstList
                || highestFirstList.Count < 1)
            {
                Debug.CheckNah("Entity null, or Rack empty or null", Indent: indent[1]);
                return null;
            }
            IPerception output = highestFirstList[0];
            Debug.CheckYeh("Got", output, Indent: indent[1]);
            return output;
        }
        public IPerception GetHighestRatedPerceptionFor(GameObject Entity)
            => GetHighestRatedPerceptionFor(Entity, true);

        public virtual bool Sense<TSense>(
            TSense Sense,
            GameObject Entity)
            where TSense : ISense<TSense>, new()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(this.Sense) + " requires a " + nameof(GameObject) + " to perceive.");

            foreach (SenseContext<TSense> context in GetSenseContexts(Sense, Entity))
                if (Sense.TrySense(context))
                    return true;

            return false;
        }

        public bool Sense<TSense>(IConcealedAction ConcealedAction, GameObject Entity)
            where TSense : ISense<TSense>, new()
        {
            if (ConcealedAction.IsNullOrEmpty()
                || Entity == null)
                return new();

            foreach (ISense sense in ConcealedAction)
                if (sense is TSense tSense
                    && Sense(tSense, Entity))
                    return true;

            return false;
        }

        public virtual bool Sense(
            ISense Sense,
            GameObject Entity)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(this.Sense) + " requires a " + nameof(GameObject) + " to perceive.");

            foreach (SenseContext context in GetSenseContexts(Entity))
                if (Sense.TrySense(context))
                    return true;

            return false;
        }

        public bool Sense(IConcealedAction ConcealedAction, GameObject Entity)
        {
            if (ConcealedAction.IsNullOrEmpty()
                || Entity == null)
                return new();

            foreach (ISense sense in ConcealedAction)
                if (Sense(sense, Entity))
                    return true;

            return false;
        }

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

            foreach (IPerception percetion in this)
                if (percetion.FireEvent(E))
                {
                    Debug.CheckNah(percetion.Name, Indent: indent[1]);
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

        public virtual bool ContainsSense<T>()
            where T : ISense<T>, new()
        {
            for (int i = 0; i < Length; i++)
                if (Items[i] is IPerception<T> typedPerception
                    && typedPerception.Sense == typeof(T))
                    return true;

            return false;
        }
        public virtual bool ContainsType(IPerception Item)
            => ContainsType(Item.GetType());

        public virtual bool Contains<T>(T Item = null)
            where T : IPerception
            => ContainsType(Item?.GetType() ?? typeof(T));

        public virtual bool Contains<T>()
            where T : ISense<T>, new()
            => ContainsSense<T>();

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

        public IEnumerable<IPerception<TSense>> AsEnumerable<TSense>(Predicate<IPerception<TSense>> Filter = null)
            where TSense : ISense<TSense>, new()
        {
            try
            {
                if (Items == null)
                    throw new InnerArrayNullException(nameof(Items));

                return Items.Where(e => e is IPerception<TSense>)?.Select(e => e as IPerception<TSense>)?.Where(Filter?.ToFunc());
            }
            catch (InnerArrayNullException)
            {
                return new IPerception<TSense>[0];
            }
        }

        public IEnumerable<SenseContext<TSense>> GetSenseContexts<TSense>(TSense Sense, GameObject Entity)
            where TSense : ISense<TSense>, new()
        {
            if (Entity == null
                && Sense == null)
                yield break;

            foreach (IPerception perception in this)
                if (perception is IPerception<TSense> tSensePerception)
                yield return new(Sense.GetIntensity(), tSensePerception, Entity);
        }

        public IEnumerable<SenseContext> GetSenseContexts(GameObject Entity)
        {
            if (Entity == null)
                yield break;

            foreach (IPerception perception in this)
                yield return new(perception, Entity);
        }

        #endregion
    }
}
