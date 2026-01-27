using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Detetections;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IAlertEvent<T> : ModPooledEvent<T>
        where T : IAlertEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public GameObject Perceiver;

        public GameObject Hider;

        public IDetectionResponseGoal Alert;

        public Event StringyEvent;

        public IAlertEvent()
        {
            Perceiver = null;
            Hider = null;

            Alert = null;

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
            Hider = null;
            Alert = null;
            StringyEvent?.Clear();
            StringyEvent = null;
        }

        public static T FromPool(
            GameObject Perceiver,
            GameObject Hider,
            IDetectionResponseGoal Alert,
            bool StringyEvent = true)
        {
            if ((Perceiver == null
                    && Hider == null)
                || FromPool() is not T E)
                return null;

            E.Perceiver = Perceiver;
            E.Hider = Hider;
            E.Alert = Alert;
            if (StringyEvent)
                E.GetStringyEvent();
            return E;
        }

        public static T HiderFromPool(
            GameObject Hider,
            IDetectionResponseGoal Alert)
        {
            if (Hider == null
                || FromPool(null, Hider, Alert, StringyEvent: false) is not T E)
                return null;

            E.GetStringyEvent();
            return E;
        }

        public static T PerceiverFromPool(
            GameObject Perceiver,
            IDetectionResponseGoal Alert)
        {
            if (Perceiver == null
                || FromPool(Perceiver, null, Alert, StringyEvent: false) is not T E)
                return null;

            E.GetStringyEvent();
            return E;
        }

        public static Event GetStringyEvent(IAlertEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID())
                .SetParameter(nameof(ForEvent.Perceiver), ForEvent?.Perceiver)
                .SetParameter(nameof(ForEvent.Hider), ForEvent?.Hider)
                .SetParameter(nameof(ForEvent.Alert), ForEvent?.Alert);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Alert)) != null)
                Alert = StringyEvent?.GetParameter<IDetectionResponseGoal>(nameof(Alert));
        }

        protected static T Process(T E, out bool Success)
        {
            Success = true;
            if (GameObject.Validate(ref E.Perceiver))
            {
                if (Success
                    && E.Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = E.Perceiver.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && E.Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Perceiver.HandleEvent(E);
            }
            if (GameObject.Validate(ref E.Hider))
            {
                if (Success
                    && E.Hider.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = E.Hider.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && E.Hider.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Hider.HandleEvent(E);
            }
            return E;
        }

        protected static T ZoneProcess(T E, out bool Success)
        {
            if (!ZoneProcess(E, ref E.Perceiver, out Success))
                ZoneProcess(E, ref E.Hider, out Success);

            return E;
        }

        protected static bool ZoneProcess(T E, ref GameObject Actor, out bool Success)
        {
            Success = true;
            if (E != null
                && GameObject.Validate(ref Actor)
                && Actor.EqualsAny(E.Perceiver, E.Hider))
            {
                if (Success)
                    Success = Actor.GetCurrentZone().FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success)
                    Success = Actor.GetCurrentZone().HandleEvent(E);

                return true;
            }
            return false;
        }

        protected static T PerceiverProcess(
            GameObject Perceiver,
            IDetectionResponseGoal Alert,
            out bool Success)
            => Process(PerceiverFromPool(Perceiver, Alert), out Success);

        protected static T HiderProcess(
            GameObject Hider,
            IDetectionResponseGoal Alert,
            out bool Success)
            => Process(HiderFromPool(Hider, Alert), out Success);
    }
}

