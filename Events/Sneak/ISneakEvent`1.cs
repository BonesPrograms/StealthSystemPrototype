using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Logging;

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
            StringyEvent?.Clear();
            StringyEvent = null;
        }

        public static T FromPool(
            GameObject Hider,
            SneakPerformance Performance)
        {
            if (Hider == null
                || FromPool() is not T E)
                return null;

            E.Hider = Hider;
            E.Performance = Performance;
            E.StringyEvent = E.GetStringyEvent();
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
            E.StringyEvent = E.GetStringyEvent();
            return E;
        }


        public static Event GetStringyEvent(ISneakEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID())
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

        protected static T Process(
            GameObject Hider,
            ref List<GameObject> Witnesses,
            SneakPerformance Performance,
            out bool Success)
        {
            Success = true;
            T E = FromPool(Hider, Performance, ref Witnesses);
            if (GameObject.Validate(ref Hider))
            {
                if (Success
                    && Hider.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = Hider.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && Hider.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = Hider.HandleEvent(E);
            }
            return E;
        }

        protected static T Process(
            GameObject Hider,
            SneakPerformance Performance,
            out bool Success)
        {
            Success = true;
            T E = FromPool(Hider, Performance);
            if (GameObject.Validate(ref Hider))
            {
                if (Success
                    && Hider.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = Hider.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && Hider.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = Hider.HandleEvent(E);
            }
            return E;
        }
    }
}

