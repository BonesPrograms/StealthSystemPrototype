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

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public partial class PerceptionRack : Rack<IPerception>
    {
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

        protected IPerception LastBestRoll;

        protected string LastBestRollEntityID;

        #region Constructors

        public PerceptionRack()
            : base()
        {
            _Owner = null;
            LastBestRoll = null;
            LastBestRollEntityID = null;
        }
        public PerceptionRack(int Capacity)
            : base(Capacity)
        {
            _Owner = null;
            LastBestRoll = null;
            LastBestRollEntityID = null;
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

        public virtual string ToString(
            string Delimiter,
            bool Short,
            GameObject Entity,
            bool UseLastRoll = false,
            bool BestRollOnly = false)
        {
            if (Items == null)
                MetricsManager.LogException(
                    Context: CallChain(nameof(PerceptionRack), nameof(ToString)),
                    x: new InnerArrayNullException(nameof(Items)),
                    category: GAME_MOD_EXCEPTION);

            if (BestRollOnly
                && LastBestRoll != null
                && Entity?.ID == LastBestRollEntityID)
                return LastBestRoll.ToString(Short: Short, Entity: Entity, UseLastRoll: UseLastRoll);

            return this?.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.ToString(Short: Short, Entity: Entity, UseLastRoll: UseLastRoll));
        }

        public virtual string ToString(
            bool Short,
            GameObject Entity = null,
            bool UseLastRoll = false,
            bool BestRollOnly = false)
            => ToString(", ", Short, Entity, UseLastRoll, BestRollOnly);

        public virtual string ToStringLines(
            bool Short = false,
            GameObject Entity = null,
            bool UseLastRoll = false,
            bool BestRollOnly = false)
            => ToString("\n", Short, Entity, UseLastRoll, BestRollOnly);

        public override string ToString()
            => ToString(Short: false, Entity: null);

        public override void Add(IPerception Item)
            => Add(Item);

        public void Add<T>(
            T Perception,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where T : IPerception, new()
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

        public T Add<T>(
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where T : IPerception, new()
        {
            T perception = new ();
            Add(perception, DoRegistration, Initial, Creation);
            return perception;
        }

        public T Add<T>(
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPerception, new()
            => Add<T>(DoRegistration, false, Creation);

        public bool Has<T>()
            where T : IPerception, new()
            => Contains<T>();

        public bool Has<T>(T Perception)
            where T : IPerception, new()
            => Contains(Perception);

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

        public T Get<T>()
            where T : IPerception, new()
        {
            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == typeof(T))
                    return Items[i] as T;
            return null;
        }

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

        public IPerception GetFirstOfSense(PerceptionSense Sense)
            => AsEnumerable(p => p.Sense == Sense)?.FirstOrDefault();

        public bool TryGet<T>(out T Perception)
            where T : IPerception, new()
            => (Perception = Get<T>()) != null;

        public bool TryGet(string Name, out IPerception Perception)
            => (Perception = Get(Name)) != null;

        public T Require<T>(
            bool Creation = false)
            where T : IPerception, new()
        {
            if (TryGet(out T perception))
                return perception;

            return Add<T>(DoRegistration: true, Creation);
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
            foreach (int index in removeList)
                RemovePerceptionAt(index);

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

        public virtual int Roll(GameObject Entity, out IPerception Perception, bool UseLastBestRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(UseLastBestRoll), UseLastBestRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            int highest = -1;
            Perception = (UseLastBestRoll && Entity?.ID == LastBestRollEntityID) ? LastBestRoll : null;

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Perception != null)
                return Perception.Roll(Entity);

            for (int i = 0; i < Length; i++)
            {
                int roll = Items[i].Roll(Entity);
                if (roll > highest)
                {
                    highest = roll;
                    Perception = LastBestRoll = Items[i];
                }
            }

            LastBestRollEntityID = Entity.ID;

            return highest;
        }
        public virtual int Roll(GameObject Entity)
            => Roll(Entity, out _);

        public virtual int RollAdvantage(GameObject Entity, out IPerception Perception, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            int first = Roll(Entity, out IPerception firstPerception, AgainstLastRoll);
            int second = Roll(Entity, out IPerception secondPerception, false);
            GetMinMax(out _, out int max, first, second);
            Perception = max == first
                ? firstPerception
                : secondPerception;
            return max;
        }
        public virtual int RollAdvantage(GameObject Entity, bool AgainstLastRoll = false)
            => RollAdvantage(Entity, out _, AgainstLastRoll);

        public virtual int RollDisadvantage(GameObject Entity, out IPerception Perception, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            int first = Roll(Entity, out IPerception firstPerception, AgainstLastRoll);
            int second = Roll(Entity, out IPerception secondPerception, false);
            GetMinMax(out int min, out _, first, second);
            Perception = min == first
                ? firstPerception
                : secondPerception;
            return min;
        }
        public virtual int RollDisadvantage(GameObject Entity, bool AgainstLastRoll = false)
            => RollDisadvantage(Entity, out _, AgainstLastRoll);

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll, out IPerception Perception, bool UseLastBestRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(UseLastBestRoll), UseLastBestRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(GetAwareness) + " requires a " + nameof(GameObject) + " to perceive.");

            Roll = this.Roll(Entity, out Perception, UseLastBestRoll);

            AwarenessLevel awarenessLevel = Perception.GetAwareness(Entity, UseLastRoll: UseLastBestRoll);

            Debug.CheckYeh(awarenessLevel.ToStringWithNum(), Indent: indent[1]);

            return awarenessLevel;
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out IPerception Perception, bool UseLastBestRoll = false)
            => GetAwareness(Entity, out _, out Perception, UseLastBestRoll);

        public virtual AwarenessLevel GetAwareness(GameObject Entity, bool UseLastBestRoll = false)
            => GetAwareness(Entity, out _, out _, UseLastBestRoll);

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

        protected static bool GameObjectHasRgisteredEventFrom(GameObject Owner, int ID, IPerception Perception)
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
                || GameObjectHasRgisteredEventFrom(Perception.Owner, ID, Perception)
                || GameObjectHasRgisteredEventFrom(Owner, ID, Perception);
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
        public virtual bool ContainsType(IPerception Item)
            => ContainsType(Item.GetType());

        public virtual bool Contains<T>(T Item = null)
            where T : IPerception
            => ContainsType(Item?.GetType() ?? typeof(T));

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

        #endregion
    }
}
