using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using StealthSystemPrototype.Logging;

using XRL;
using XRL.Collections;
using XRL.World;

using static StealthSystemPrototype.Capabilities.Stealth.BasePerception;
using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Const;
using XRL.World.Parts;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public partial class PerceptionRack : Rack<BasePerception>
    {
        [UD_DebugRegistry]
        public static List<MethodRegistryEntry> doDebugRegistry(List<MethodRegistryEntry> Registry)
        {
            Dictionary<string, bool> multiMethodRegistrations = new()
            {
                { nameof(Add), false },
                { nameof(HasWantEvent), false },
                { nameof(PerceptionWantsEvent), false },
            };
            UnityEngine.Debug.Log(CallChain(nameof(PerceptionRack), nameof(doDebugRegistry)));
            typeof(PerceptionRack)?.GetMethods()?.ToList()?.ForEach(mi => UnityEngine.Debug.Log(mi.Name));

            foreach (MethodBase perceptionRackMethod in typeof(PerceptionRack).GetMethods() ?? new MethodBase[0])
                if (multiMethodRegistrations.ContainsKey(perceptionRackMethod.Name))
                    Registry.Register(perceptionRackMethod, multiMethodRegistrations[perceptionRackMethod.Name]);

            return Registry;
        }

        public GameObject Owner;

        protected BasePerception LastBestRoll;

        protected string LastBestRollEntityID;

        protected bool CollectingPerceptions => Owner?.GetPart<UD_PerceptionHelper>()?.CollectingPerceptions ?? false;

        #region Constructors

        public PerceptionRack()
            : base()
        {
            Owner = null;
            LastBestRoll = null;
            LastBestRollEntityID = null;
        }
        public PerceptionRack(int Capacity)
            : base(Capacity)
        {
            Owner = null;
            LastBestRoll = null;
            LastBestRollEntityID = null;
        }
        public PerceptionRack(GameObject Owner)
            : this()
        {
            this.Owner = Owner;
        }
        public PerceptionRack(GameObject Owner, int Capacity)
            : this(Capacity)
        {
            this.Owner = Owner;
        }
        public PerceptionRack(IReadOnlyCollection<BasePerception> Source)
            : this(Source.Count)
        {
            Length = Source.Count;
            Items = Source.ToArray();
        }
        public PerceptionRack(GameObject Owner, IReadOnlyCollection<BasePerception> Source)
            : this(Source)
        {
            this.Owner = Owner;
        }
        public PerceptionRack(PerceptionRack Source)
            : this(Source.Owner, (IReadOnlyCollection<BasePerception>)Source)
        {
        }
        public PerceptionRack(GameObject NewOwner, PerceptionRack Source)
            : this(Source)
        {
            Owner = NewOwner;
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

        public override void Add(BasePerception Item)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Item), Item?.Name ?? "null"),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                });

            Add(Item, true);
        }

        public void Add<T>(T Item, bool Override)
            where T : BasePerception
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Item), Item?.Name ?? "null"),
                    Debug.Arg(nameof(Override), Override),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                });

            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            if (Override
                || !Contains(Item))
            {
                Remove(Item);

                Debug.Log("Calling base." + nameof(Add), Indent: indent[1]);
                base.Add(Item);

                if (Item.Owner != Owner)
                    Item.Owner = Owner;
            }
        }

        public bool RemoveType(Type Type)
        {
            if (Type == null)
                throw new ArgumentNullException(nameof(Type), "Cannot be null.");

            int index = -1;
            for (int i = 0; i < Count; i++)
                if (Items[i].GetType() == Type)
                {
                    index = i;
                    break;
                }

            if (index < 0)
                return false;

            Length--;

            if (index < Count)
                Array.Copy(Items, index + 1, Items, index, Count - index);

            Items[Count] = null;
            Variant++;

            return true;
        }

        public bool RemoveType(BasePerception Item)
            => RemoveType(Item?.GetType());

        public bool Remove<T>(T Item)
            where T : BasePerception
            => RemoveType(Item?.GetType() ?? typeof(T));

        public bool Validate(GameObject Owner = null, bool RemoveInvalid = true)
        {
            Owner ??= this.Owner;
            bool allValid = true;
            List<BasePerception> removeList = new();
            for (int i = 0; i < Count; i++)
            {
                if (Items[i] is not BasePerception perception)
                    throw new InvalidOperationException(nameof(Items) + " contains null entry at " + i + " despite length of " + Count + ".");

                if (!perception.Validate(Owner))
                {
                    if (RemoveInvalid)
                        removeList.Add(perception);
                    else
                        allValid = false;
                }
            }
            foreach (BasePerception perception in removeList)
                RemoveType(perception);

            removeList.Clear();

            if (RemoveInvalid
                && !allValid)
                allValid = Validate(Owner, false);

            return allValid;
        }

        public void ClearRatings()
        {
            for (int i = 0; i < Count; i++)
                Items[i].ClearRating();
        }

        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity,
            Predicate<BasePerception> Filter,
            bool ClearFirst)
        {
            if (Items == null)
                throw new InnerArrayNullException(nameof(Items));

            if (Items.ToList() is not List<BasePerception> perceptionsList)
                return null;

            if (ClearFirst)
                ClearRatings();

            perceptionsList.Sort(new RatingComparer(Entity));

            return perceptionsList
                ?.Where(Filter.ToFunc());
        }
        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity,
            Predicate<BasePerception> Filter)
            => GetPerceptionsBestFirst(Entity, Filter, true);

        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity,
            bool ClearFirst)
            => GetPerceptionsBestFirst(Entity, null, ClearFirst);

        public IEnumerable<BasePerception> GetPerceptionsBestFirst(
            GameObject Entity)
            => GetPerceptionsBestFirst(Entity, null);

        public BasePerception GetHighestRatedPerceptionFor(GameObject Entity, bool ClearFirst)
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
                || GetPerceptionsBestFirst(Entity, ClearFirst) is not List<BasePerception> highestFirstList
                || highestFirstList.Count < 1)
            {
                Debug.CheckNah("Entity null, or Rack empty or null", Indent: indent[1]);
                return null;
            }
            BasePerception output = highestFirstList[0];
            Debug.CheckYeh("Got", output, Indent: indent[1]);
            return output;
        }
        public BasePerception GetHighestRatedPerceptionFor(GameObject Entity)
            => GetHighestRatedPerceptionFor(Entity, true);

        public virtual int Roll(GameObject Entity, out BasePerception Perception, bool UseLastBestRoll = false)
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

        public virtual int RollAdvantage(GameObject Entity, out BasePerception Perception, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            int first = Roll(Entity, out BasePerception firstPerception, AgainstLastRoll);
            int second = Roll(Entity, out BasePerception secondPerception, false);
            GetMinMax(out _, out int max, first, second);
            Perception = max == first
                ? firstPerception
                : secondPerception;
            return max;
        }
        public virtual int RollAdvantage(GameObject Entity, bool AgainstLastRoll = false)
            => RollAdvantage(Entity, out _, AgainstLastRoll);

        public virtual int RollDisadvantage(GameObject Entity, out BasePerception Perception, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            int first = Roll(Entity, out BasePerception firstPerception, AgainstLastRoll);
            int second = Roll(Entity, out BasePerception secondPerception, false);
            GetMinMax(out int min, out _, first, second);
            Perception = min == first
                ? firstPerception
                : secondPerception;
            return min;
        }
        public virtual int RollDisadvantage(GameObject Entity, bool AgainstLastRoll = false)
            => RollDisadvantage(Entity, out _, AgainstLastRoll);

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll, out BasePerception Perception, bool UseLastBestRoll = false)
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

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out BasePerception Perception, bool UseLastBestRoll = false)
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

        protected static bool GameObjectHasRgisteredEventFrom(GameObject Owner, int ID, BasePerception Perception)
            => Owner != null
                && Owner.HasRegisteredEventFrom(ID, Perception);

        public static bool PerceptionWantsEvent(
            int ID,
            int Cascade,
            BasePerception Perception,
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

            if (CollectingPerceptions)
                return false;

            if (MinEvent.CascadeTo(Cascade, MinEvent.CASCADE_NONE))
                return false;

            if (Owner?.RegisteredEvents?.ContainsKey(ID)
                ?? false)
                return true;

            foreach (BasePerception perception in this)
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

            if (CollectingPerceptions)
                return true;

            if (E.CascadeTo(MinEvent.CASCADE_NONE))
                return true;

            foreach (BasePerception perception in this)
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

            foreach (BasePerception percetion in this)
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
        public virtual bool ContainsType(BasePerception Item)
            => ContainsType(Item.GetType());

        public virtual bool Contains<T>(T Item)
            where T : BasePerception
            => ContainsType(Item?.GetType() ?? typeof(T));

        #endregion
        #region Conversion Methods

        public IEnumerable<BasePerception> AsEnumerable(Predicate<BasePerception> Filter = null)
        {
            try
            {
                if (Items == null)
                    throw new InnerArrayNullException(nameof(Items));

                return Items.Where(Filter?.ToFunc());
            }
            catch (InnerArrayNullException)
            {
                return new BasePerception[0];
            }
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.WriteOptimized(Variant);
        }

        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Variant = Reader.ReadOptimizedInt32();
        }

        #endregion
    }
}
