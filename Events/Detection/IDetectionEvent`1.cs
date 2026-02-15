using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IDetectionEvent<T> : ModPooledEvent<T>
        where T : IDetectionEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public GameObject Perceiver;

        public GameObject Sneaker;

        public IOpinionDetection Detection;

        public Event StringyEvent;

        public IDetectionEvent()
        {
            Perceiver = null;
            Sneaker = null;

            Detection = null;

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
            Sneaker = null;
            Detection = null;
            StringyEvent?.Clear();
            StringyEvent = null;
        }

        public static T FromPool(
            GameObject Perceiver,
            GameObject Sneaker,
            IOpinionDetection Detection,
            bool StringyEvent = true)
        {
            if ((Perceiver == null
                    && Sneaker == null)
                || FromPool() is not T E)
                return null;

            E.Perceiver = Perceiver;
            E.Sneaker = Sneaker;
            E.Detection = Detection;
            if (StringyEvent)
                E.GetStringyEvent();
            return E;
        }

        public static T SneakerFromPool(
            GameObject Sneaker,
            IOpinionDetection Detection)
        {
            if (Sneaker == null
                || FromPool(null, Sneaker, Detection, StringyEvent: false) is not T E)
                return null;

            E.GetStringyEvent();
            return E;
        }

        public static T PerceiverFromPool(
            GameObject Perceiver,
            IOpinionDetection Detection)
        {
            if (Perceiver == null
                || FromPool(Perceiver, null, Detection, StringyEvent: false) is not T E)
                return null;

            E.GetStringyEvent();
            return E;
        }

        public static Event GetStringyEvent(IDetectionEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : (ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID()))
                .SetParameter(nameof(ForEvent.Perceiver), ForEvent?.Perceiver)
                .SetParameter(nameof(ForEvent.Sneaker), ForEvent?.Sneaker)
                .SetParameter(nameof(ForEvent.Detection), ForEvent?.Detection);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Detection)) != null)
                Detection = StringyEvent?.GetParameter<IOpinionDetection>(nameof(Detection));
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
            if (GameObject.Validate(ref E.Sneaker))
            {
                if (Success
                    && E.Sneaker.HasRegisteredEvent(E.GetRegisteredEventID()))
                    Success = E.Sneaker.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && E.Sneaker.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Sneaker.HandleEvent(E);
            }
            return E;
        }

        protected static T ZoneProcess(T E, out bool Success)
        {
            if (!ZoneProcess(E, ref E.Perceiver, out Success))
                ZoneProcess(E, ref E.Sneaker, out Success);

            return E;
        }

        protected static bool ZoneProcess(T E, ref GameObject Actor, out bool Success)
        {
            Success = true;
            if (E != null
                && GameObject.Validate(ref Actor)
                && Actor.EqualsAny(E.Perceiver, E.Sneaker))
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
            IOpinionDetection Detection,
            out bool Success)
            => Process(PerceiverFromPool(Perceiver, Detection), out Success);

        protected static T SneakerProcess(
            GameObject Sneaker,
            IOpinionDetection Detection,
            out bool Success)
            => Process(SneakerFromPool(Sneaker, Detection), out Success);
    }
}