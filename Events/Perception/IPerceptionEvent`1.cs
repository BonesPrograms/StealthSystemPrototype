using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IPerceptionEvent<T> : ModPooledEvent<T>
        where T : IPerceptionEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public GameObject Perceiver;

        protected IPerception Perception;

        public PerceptionRack Perceptions;

        public Event StringyEvent;

        public IPerceptionEvent()
        {
            Perceiver = null;
            Perception = null;
            Perceptions = null;

            StringyEvent = null;
        }

        public virtual string GetRegisteredEventID()
            => RegisteredEventID;

        public override int GetCascadeLevel()
            => CascadeLevel;

        public override void Reset()
        {
            base.Reset();
            Perceiver = null;
            Perception = null;
            Perceptions = null;
            StringyEvent?.Clear();
            StringyEvent = null;
        }

        public static T FromPool(
            GameObject Perciever,
            IPerception Perception,
            PerceptionRack Perceptions)
        {
            if (Perciever == null
                || FromPool() is not T E)
                return null;
            
            E.Perceiver = Perciever;
            E.Perception = Perception;
            E.Perceptions = Perceptions;
            E.StringyEvent = E.GetStringyEvent();
            return E;
        }

        public static T FromPool(GameObject Perciever, IPerception Perception)
            => FromPool(Perciever, Perception, null);

        public static T FromPool(GameObject Perciever, PerceptionRack Perceptions)
            => FromPool(Perciever, null, Perceptions);

        public static T FromPool(GameObject Perciever)
            => FromPool(Perciever, null, null);

        public static Event GetStringyEvent(IPerceptionEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : (ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID()))
                .SetParameter(nameof(ForEvent.Perceiver), ForEvent?.Perceiver)
                .SetParameterOrNullExisting(nameof(ForEvent.Perception), ForEvent.Perception)
                .SetParameterOrNullExisting(nameof(ForEvent.Perceptions), ForEvent.Perceptions);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Perceptions)) is PerceptionRack perceptions)
                Perceptions = perceptions;
        }

        protected static T Process(
            GameObject Perciever,
            IPerception Perception,
            PerceptionRack Perceptions,
            out bool Success)
        {
            Success = true;
            T E = FromPool(Perciever, Perception, Perceptions);
            if (GameObject.Validate(ref Perciever))
            {
                if (Success
                    && Perciever.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = Perciever.FireEvent(E.StringyEvent);

                if (Success)
                    E.UpdateFromStringyEvent();

                if (Success
                    && Perciever.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = Perciever.HandleEvent(E);
            }
            return E;
        }

        public virtual string GetPerceptionName(bool Short = false)
            => Perception?.GetName(Short);

        public virtual bool CanPerceptionPerceive(IAlert Alert)
            => Perception?.CanPerceiveAlert(Alert) ?? false;
    }
}

