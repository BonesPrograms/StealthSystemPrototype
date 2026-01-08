using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using static StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Events.Perception
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IPerceptionEvent<T> : ModPooledEvent<T>
        where T : IPerceptionEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public Event StringyEvent;

        public GameObject Perciever;

        public Dictionary<string, PerceptionScore> PerceptionScores;

        public IPerceptionEvent()
        {
            StringyEvent = null;
            Perciever = null;
            PerceptionScores = null;
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
            PerceptionScores = null;
        }

        public static T FromPool(GameObject Perciever, Dictionary<string, PerceptionScore> PerceptionScores = null)
        {
            if (Perciever == null)
                return FromPool();

            T E = FromPool();
            E.StringyEvent = E.GetStringyEvent();
            E.Perciever = Perciever;
            E.PerceptionScores = PerceptionScores ?? new();
            return E;
        }

        public static Event GetStringyEvent(IPerceptionEvent<T> ForEvent)
            => ForEvent == null
            ? Event.New(RegisteredEventID)
            : Event.New(ForEvent.GetRegisteredEventID(),
                nameof(ForEvent.Perciever), ForEvent?.Perciever,
                nameof(ForEvent.PerceptionScores), ForEvent?.PerceptionScores);

        public virtual Event GetStringyEvent()
            => StringyEvent = GetStringyEvent(this);

        public virtual void UpdateFromStringyEvent(bool ClearStringyAfter = false)
        {
            if (StringyEvent?.GetParameter(nameof(PerceptionScores)) is Dictionary<string, PerceptionScore> perceptionScores)
                PerceptionScores = perceptionScores;
            
            if (ClearStringyAfter)
                StringyEvent.Clear();
        }

        protected static T Process(GameObject Perciever, Dictionary<string, PerceptionScore> PerceptionScores, out bool Success)
        {
            Success = true;
            T E = FromPool(Perciever, PerceptionScores);
            if (Success
                && GameObject.Validate(ref Perciever)
                && Perciever.HasRegisteredEvent(E.GetRegisteredEventID()))
            {
                Success = Perciever.FireEvent(E.StringyEvent);
            }
            if (Success
                && GameObject.Validate(ref Perciever)
                && Perciever.WantEvent(E.GetID(), E.GetCascadeLevel()))
            {
                Success = Perciever.HandleEvent(E);
            }
            return E;
        }

        
    }
}

