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

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class ISneakEvent<T> : ModPooledEvent<T>
        where T : ISneakEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public GameObject Hider;

        public SneakPerformance Performance;

        public List<GameObject> Witnesses;

        public Event StringyEvent;

        public ISneakEvent()
        {
            Hider = null;
            Performance = null;
            Witnesses = null;

            StringyEvent = null;
        }

        public virtual string GetRegisteredEventID()
            => RegisteredEventID;

        public override int GetCascadeLevel()
            => CascadeLevel;

        public override void Reset()
        {
            base.Reset();
            Hider = null;
            Performance = null;
            Witnesses = null;
            StringyEvent = null;
        }

        public static T FromPool(
            GameObject Hider)
        {
            if (Hider == null
                || FromPool() is not T E)
                return null;

            E.Hider = Hider;
            E.GetStringyEvent();
            return E;
        }

        public static T FromPool(
            GameObject Hider,
            ref SneakPerformance Performance)
        {
            if (Hider == null
                || FromPool(Hider) is not T E)
                return null;

            E.Performance = (Performance ??= new(E.Hider));
            E.GetStringyEvent();
            return E;
        }

        public static T FromPool(
            GameObject Hider,
            SneakPerformance Performance)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.MiniDebugName() ?? "null"),
                    Debug.Arg(nameof(Performance), Performance?.Count ?? -1),
                });

            if (Hider == null
                || FromPool(Hider) is not T E)
                return null;

            E.Performance = Performance;
            E.GetStringyEvent();
            return E;
        }

        public static T FromPool(
            GameObject Hider,
            SneakPerformance Performance,
            ref List<GameObject> Witnesses,
            bool CollectWitnesses = false)
        {
            if (Hider == null
                || FromPool(Hider, Performance) is not T E)
                return null;

            if (CollectWitnesses)
                GetWitnessesEvent.GetFor(E.Hider, ref Witnesses);
            E.Witnesses = Witnesses;
            E.GetStringyEvent();
            return E;
        }

        public static Event GetStringyEvent(ISneakEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : (ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID()))
                .SetParameter(nameof(ForEvent.Hider), ForEvent?.Hider)
                .SetParameterOrNullExisting(nameof(ForEvent.Performance), ForEvent.Performance)
                .SetParameterOrNullExisting(nameof(ForEvent.Witnesses), ForEvent.Witnesses);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Witnesses)) != null)
                Performance = StringyEvent?.GetParameter<SneakPerformance>(nameof(Performance));

            if (StringyEvent?.GetParameter(nameof(Witnesses)) != null)
                Witnesses = StringyEvent?.GetParameter<List<GameObject>>(nameof(Witnesses));
        }

        protected static T Process(T E, out bool Success)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.TypeStringWithGenerics()),
                    Debug.Arg(E.Hider?.DebugName ?? "null"),
                    Debug.Arg(nameof(E.Performance), E.Performance?.Count ?? -1),
                });

            if (E == null)
            {
                Success = false;
                return null;
            }

            Success = true;
            if (GameObject.Validate(ref E.Hider))
            {
                Debug.YehNah(nameof(GameObject.Validate), Success, Indent: indent[1]);

                if (Success
                    && E.Hider.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = E.Hider.FireEvent(E.StringyEvent);
                Debug.YehNah(nameof(GameObject.FireEvent), Success, Indent: indent[1]);

                E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), Success, Indent: indent[1]);

                if (Success
                    && E.Hider.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Hider.HandleEvent(E);
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
                    Debug.Arg(E.Hider?.DebugName ?? "null"),
                    Debug.Arg(nameof(E.Performance), E.Performance?.Count ?? -1),
                });

            if (E == null)
            {
                Success = false;
                return null;
            }

            Success = true;
            if (GameObject.Validate(ref E.Hider))
            {
                if (Success)
                    Success = E.Hider.GetCurrentZone().FireEvent(E.StringyEvent);
                Debug.YehNah(nameof(Zone.FireEvent), Success, Indent: indent[1]);

                if (Success)
                    E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), Success, Indent: indent[1]);

                if (Success)
                    Success = E.Hider.GetCurrentZone().HandleEvent(E);
                Debug.YehNah(nameof(Zone.HandleEvent), Success, Indent: indent[1]);
            }
            return E;
        }

        protected static T Process(
            GameObject Hider,
            ref SneakPerformance Performance,
            out bool Success)
            => Process(FromPool(Hider, ref Performance), out Success);

        protected static T Process(
            GameObject Hider,
            ref List<GameObject> Witnesses,
            SneakPerformance Performance,
            out bool Success)
            => Process(FromPool(Hider, Performance, ref Witnesses), out Success);
    }
}

