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

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IWitnessEvent<T> : ModPooledEvent<T>
        where T : IWitnessEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public GameObject Hider;

        public List<GameObject> Witnesses;

        public Event StringyEvent;

        public IWitnessEvent()
        {
            Hider = null;
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
            Witnesses = null;
            StringyEvent?.Clear();
            StringyEvent = null;
        }

        public static T FromPool(GameObject Hider, List<GameObject> Witnesses = null)
        {
            if (Hider == null
                || FromPool() is not T E)
                return null;

            E.Hider = Hider;
            E.Witnesses = Witnesses ?? Event.NewGameObjectList();
            E.StringyEvent = E.GetStringyEvent();
            return E;
        }

        public static Event GetStringyEvent(IWitnessEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : (ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID()))
                .SetParameter(nameof(ForEvent.Hider), ForEvent?.Hider)
                .SetParameterOrNullExisting(nameof(ForEvent.Witnesses), ForEvent.Witnesses);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Witnesses)) != null)
                Witnesses = StringyEvent?.GetParameter(nameof(Witnesses)) as List<GameObject>;
        }

        protected static T Process(
            GameObject Hider,
            List<GameObject> Witnesses,
            out bool Success)
        {
            Success = true;
            T E = FromPool(Hider, Witnesses);
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

