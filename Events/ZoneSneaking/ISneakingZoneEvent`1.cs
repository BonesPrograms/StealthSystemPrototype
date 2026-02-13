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
using XRL.World.ZoneParts;
using System.Linq;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class ISneakingZoneEvent<T> : ModPooledEvent<T>
        where T : ISneakingZoneEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_STOP_AT_ZONE | CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public Zone Zone;

        public UD_SneakWitnesser SneakWitnesser => Zone?.RequirePart<UD_SneakWitnesser>();

        public Witnesses Witnesses;

        public Sneakers Sneakers;

        public Event StringyEvent;

        public ISneakingZoneEvent()
        {
            Zone = null;
            Witnesses = null;
            Sneakers = null;
            StringyEvent = null;
        }

        public virtual string GetRegisteredEventID()
            => RegisteredEventID;

        public override int GetCascadeLevel()
            => CascadeLevel;

        public override void Reset()
        {
            base.Reset();
            Witnesses = null;
            Sneakers = null;
            StringyEvent?.Clear();
            StringyEvent = null;
        }

        protected static T FromPool(Zone Zone, out UD_SneakWitnesser SneakWitnesser)
        {
            SneakWitnesser = null;
            if (Zone.Validate(ref Zone)
                || (SneakWitnesser = Zone.RequirePart<UD_SneakWitnesser>()) == null
                || FromPool() is not T E)
                return null;

            E.Zone = Zone;
            E.StringyEvent = E.GetStringyEvent();
            return E;
        }

        public static T FromPool(Zone Zone, ref Witnesses Witnesses)
        {
            if (FromPool(Zone, out UD_SneakWitnesser sneakWitnesser) is not T E)
                return null;

            Witnesses ??= new(sneakWitnesser);
            if (Witnesses.ParentPart != sneakWitnesser)
                Witnesses = new(sneakWitnesser, Witnesses);

            E.Witnesses = Witnesses;
            E.GetStringyEvent();
            return E;
        }

        public static T FromPool(Zone Zone, ref Sneakers Sneakers)
        {
            if (FromPool(Zone, out UD_SneakWitnesser sneakWitnesser) is not T E)
                return null;

            Sneakers ??= new(sneakWitnesser);
            if (Sneakers.ParentPart != sneakWitnesser)
                Sneakers = new(sneakWitnesser, Sneakers);

            E.Sneakers = Sneakers;
            E.GetStringyEvent();
            return E;
        }

        public static Event GetStringyEvent(ISneakingZoneEvent<T> ForEvent, ref Event ExistingEvent)
            => ForEvent == null
            ? ExistingEvent = Event.New(RegisteredEventID)
            : (ExistingEvent ??= Event.New(ForEvent.GetRegisteredEventID()))
                .SetParameter(nameof(ForEvent.Zone), ForEvent?.Zone)
                .SetParameterOrNullExisting(nameof(ForEvent.Witnesses), ForEvent.Witnesses)
                .SetParameterOrNullExisting(nameof(ForEvent.Sneakers), ForEvent.Sneakers);

        public virtual Event GetStringyEvent()
            => GetStringyEvent(this, ref StringyEvent);

        public virtual void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetParameter(nameof(Witnesses)) is Witnesses witnesses)
                Witnesses = witnesses;

            if (StringyEvent?.GetParameter(nameof(Sneakers)) is Sneakers sneakers)
                Sneakers = sneakers;
        }

        protected static T Process(T E, out bool Success)
        {
            if (E == null)
            {
                Success = false;
                return null;
            }
            Success = true;
            if (Zone.Validate(ref E.Zone))
            {
                if (Success)
                    Success = E.Zone.FireEvent(E.StringyEvent);

                E.UpdateFromStringyEvent();

                if (Success
                    && E.Zone.WantEvent(E.GetID(), E.GetCascadeLevel()))
                    Success = E.Zone.HandleEvent(E);
            }
            return E;
        }

        protected static T Process(
            Zone Zone,
            ref Witnesses Witnesses,
            out bool Success
            )
            => Process(FromPool(Zone, ref Witnesses), out Success);

        protected static T Process(
            Zone Zone,
            ref Sneakers Sneakers,
            out bool Success
            )
            => Process(FromPool(Zone, ref Sneakers), out Success);

        protected static T WitnessesProcess(T E, out bool Success)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.TypeStringWithGenerics()),
                });

            if (E == null)
            {
                Success = false;
                return null;
            }

            Success = true;
            if (!E.Witnesses.IsNullOrEmpty())
            {
                if (Success)
                    Success = E.Witnesses.FireEvent(E.StringyEvent, RegisteredOnly: true);
                Debug.YehNah(nameof(Extensions.FireEvent), Success, Indent: indent[1]);

                if (Success)
                    E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), Success, Indent: indent[1]);

                if (Success)
                    Success = E.Witnesses.HandleEvent(E, WantOnly: true);
                Debug.YehNah(nameof(Extensions.HandleEvent), Success, Indent: indent[1]);
            }
            return E;
        }

        protected static T SneakersProcess(T E, out bool Success)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.TypeStringWithGenerics()),
                });

            if (E == null)
            {
                Success = false;
                return null;
            }

            Success = true;
            if (E.Sneakers.Select(ss => ss.Sneaker) is IEnumerable<GameObject> sneakers
                && sneakers.IsNullOrEmpty())
            {
                if (Success)
                    Success = sneakers.FireEvent(E.StringyEvent, RegisteredOnly: true);
                Debug.YehNah(nameof(Extensions.FireEvent), Success, Indent: indent[1]);

                if (Success)
                    E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), Success, Indent: indent[1]);

                if (Success)
                    Success = sneakers.HandleEvent(E, WantOnly: true);
                Debug.YehNah(nameof(Extensions.HandleEvent), Success, Indent: indent[1]);
            }
            return E;
        }
    }
}

