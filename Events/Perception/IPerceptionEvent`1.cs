using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;

using XRL.Collections;
using XRL.World;
using XRL.World.Parts;

using static StealthSystemPrototype.Capabilities.Stealth.Perception2;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IPerceptionEvent<T> : ModPooledEvent<T>
        where T : IPerceptionEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public Event StringyEvent;

        public GameObject Perciever;

        public BasePerception Perception;

        public Perceptions Perceptions;

        public IPerceptionEvent()
        {
            StringyEvent = null;
            Perciever = null;
            Perception = null;
            Perceptions = null;
        }

        public virtual string GetRegisteredEventID()
            => RegisteredEventID;

        public override int GetCascadeLevel()
            => CascadeLevel;

        public override void Reset()
        {
            base.Reset();
            StringyEvent?.Clear();
            StringyEvent = null;
            Perciever = null;
            Perception = null;
            Perceptions?.Clear();
            Perceptions = null;
        }

        public static T FromPool(GameObject Perciever, BasePerception Perception, Perceptions Perceptions)
        {
            if (Perciever == null)
                return FromPool();

            T E = FromPool();
            E.StringyEvent = E.GetStringyEvent();
            E.Perciever = Perciever;
            E.Perception = Perception;
            E.Perceptions = Perceptions;
            return E;
        }

        public static T FromPool(GameObject Perciever, BasePerception Perception)
            => FromPool(Perciever, Perception, null);

        public static T FromPool(GameObject Perciever, Perceptions Perceptions)
            => FromPool(Perciever, null, Perceptions);

        public static T FromPool(GameObject Perciever)
            => FromPool(Perciever, null, null);

        public static Event GetStringyEvent(IPerceptionEvent<T> ForEvent)
        {
            if (ForEvent == null)
                return Event.New(RegisteredEventID);

            var @event = Event.New(ForEvent.GetRegisteredEventID(),
                nameof(ForEvent.Perciever), ForEvent?.Perciever);

            if (ForEvent.Perception != null)
                @event.AddParameter(nameof(ForEvent.Perception), ForEvent?.Perception);

            if (ForEvent.Perceptions != null)
                @event.AddParameter(nameof(ForEvent.Perceptions), ForEvent?.Perceptions);

            return @event;
        }

        public virtual Event GetStringyEvent()
            => StringyEvent = GetStringyEvent(this);

        public virtual void UpdateFromStringyEvent(bool ClearStringyAfter = false)
        {
            if (StringyEvent?.GetParameter(nameof(Perception)) is Perceptions perception)
                Perceptions = perception;

            if (StringyEvent?.GetParameter(nameof(Perceptions)) is Perceptions perceptions)
                Perceptions = perceptions;
            
            if (ClearStringyAfter)
                StringyEvent.Clear();
        }

        protected static T Process(
            GameObject Perciever,
            BasePerception Perception,
            Perceptions Perceptions,
            out bool Success)
        {
            Success = true;
            T E = FromPool(Perciever, Perception, Perceptions);
            if (GameObject.Validate(ref Perciever))
            {
                if (Success
                    && Perciever.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = Perciever.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent(true);

                if (Success
                    && Perciever.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = Perciever.HandleEvent(E);
            }
            return E;
        }
    }
}

