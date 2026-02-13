using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using XRL.World.ZoneParts;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class ISneakPerformanceEvent<T> : ModPooledEvent<T>
        where T : ISneakPerformanceEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public GameObject Sneaker;

        public SneakPerformance Performance;

        public Event StringyEvent;

        public ISneakPerformanceEvent()
        {
            Sneaker = null;
            Performance = null;

            StringyEvent = null;
        }

        public virtual string GetRegisteredEventID()
            => RegisteredEventID;

        public override int GetCascadeLevel()
            => CascadeLevel;

        public override void Reset()
        {
            base.Reset();
            Sneaker = null;
            Performance = null;
            StringyEvent = null;
        }

        public static T FromPool(
            GameObject Sneaker)
        {
            if (Sneaker == null
                || FromPool() is not T E)
                return null;

            E.Sneaker = Sneaker;
            E.GetStringyEvent();
            return E;
        }

        public static T FromPool(
            GameObject Sneaker,
            ref SneakPerformance Performance)
        {
            if (Sneaker == null
                || FromPool(Sneaker) is not T E)
                return null;

            E.Performance = (Performance ??= new(E.Sneaker));
            E.GetStringyEvent();
            return E;
        }

        public static T FromPool(
            GameObject Sneaker,
            SneakPerformance Performance)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Sneaker?.MiniDebugName() ?? "null"),
                    Debug.Arg(nameof(Performance), Performance?.Count ?? -1),
                });

            if (Sneaker == null
                || FromPool(Sneaker) is not T E)
                return null;

            E.Performance = Performance;
            E.GetStringyEvent();
            return E;
        }

        public static Event GetStringyEvent(ISneakPerformanceEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : (ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID()))
                .SetParameter(nameof(ForEvent.Sneaker), ForEvent?.Sneaker)
                .SetParameterOrNullExisting(nameof(ForEvent.Performance), ForEvent.Performance);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Witnesses)) != null)
                Performance = StringyEvent?.GetParameter<SneakPerformance>(nameof(Performance));
        }

        protected static T Process(T E, out bool Success)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.TypeStringWithGenerics()),
                    Debug.Arg(E.Sneaker?.DebugName ?? "null"),
                    Debug.Arg(nameof(E.Performance), E.Performance?.Count ?? -1),
                });

            if (E == null)
            {
                Success = false;
                return null;
            }

            Success = true;
            if (GameObject.Validate(ref E.Sneaker))
            {
                Debug.YehNah(nameof(GameObject.Validate), Success, Indent: indent[1]);

                if (Success
                    && E.Sneaker.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = E.Sneaker.FireEvent(E.StringyEvent);
                Debug.YehNah(nameof(GameObject.FireEvent), Success, Indent: indent[1]);

                E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), Success, Indent: indent[1]);

                if (Success
                    && E.Sneaker.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Sneaker.HandleEvent(E);
                Debug.YehNah(nameof(GameObject.HandleEvent), Success, Indent: indent[1]);
            }
            Debug.YehNah(nameof(Process), Success, Indent: indent[0]);
            return E;
        }

        protected static T ZoneProcess(T E, out bool Success)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.TypeStringWithGenerics()),
                    Debug.Arg(E.Sneaker?.DebugName ?? "null"),
                    Debug.Arg(nameof(E.Performance), E.Performance?.Count ?? -1),
                });

            if (E == null)
            {
                Success = false;
                return null;
            }

            Success = true;
            if (GameObject.Validate(ref E.Sneaker))
            {
                Zone zone = E.Sneaker.GetCurrentZone();

                if (Success)
                    Success = zone.FireEvent(E.StringyEvent);
                Debug.YehNah(nameof(Zone.FireEvent), Success, Indent: indent[1]);

                if (Success)
                    E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), Success, Indent: indent[1]);

                if (Success)
                    Success = !zone.WantEvent(E.GetID(), E.GetCascadeLevel())
                        || zone.HandleEvent(E);
                Debug.YehNah(nameof(Zone.HandleEvent), Success, Indent: indent[1]);
            }
            return E;
        }

        protected static T Process(
            GameObject Sneaker,
            ref SneakPerformance Performance,
            out bool Success)
            => Process(FromPool(Sneaker, ref Performance), out Success);

        protected static T Process(
            GameObject Sneaker,
            SneakPerformance Performance,
            out bool Success)
            => Process(FromPool(Sneaker, Performance), out Success);
    }
}

