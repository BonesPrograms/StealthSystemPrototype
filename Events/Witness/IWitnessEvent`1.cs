using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

namespace StealthSystemPrototype.Events.Witness
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public abstract class IWitnessEvent<T> : ModPooledEvent<T>
        where T : IWitnessEvent<T>, new()
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public static string RegisteredEventID => typeof(T).Name;

        public Event StringyEvent;

        public GameObject Hider;

        public List<GameObject> Witnesses;

        public IWitnessEvent()
        {
            StringyEvent = null;
            Hider = null;
            Witnesses = null;
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
            Hider = null;
            Witnesses = null;
        }

        public static T FromPool(GameObject Hider, List<GameObject> Witnesses = null)
        {
            if (Hider == null)
            {
                return FromPool();
            }
            T E = FromPool();
            E.GetStringyEvent();
            E.Hider = Hider;
            E.Witnesses = Witnesses ?? Event.NewGameObjectList();
            return E;
        }

        public static Event GetStringyEvent(IWitnessEvent<T> ForEvent)
            => ForEvent == null
            ? Event.New(RegisteredEventID)
            : Event.New(ForEvent.GetRegisteredEventID(),
                nameof(ForEvent.Hider), ForEvent?.Hider,
                nameof(ForEvent.Witnesses), ForEvent?.Witnesses);

        public virtual Event GetStringyEvent()
            => StringyEvent = GetStringyEvent(this);

        public virtual void UpdateFromStringyEvent(bool ClearStringyAfter = false)
        {
            Witnesses ??= Event.NewGameObjectList();

            if (StringyEvent?.GetParameter(nameof(Witnesses)) != null)
                Witnesses.AddRange(StringyEvent?.GetParameter(nameof(Witnesses)) as List<GameObject>);

            if (ClearStringyAfter)
                StringyEvent.Clear();
        }

        protected static T Process(GameObject Hider, List<GameObject> Witnesses, out bool Success)
        {
            Success = true;
            T E = FromPool(Hider, Witnesses);
            if (Success
                && GameObject.Validate(ref Hider)
                && Hider.HasRegisteredEvent(E.GetRegisteredEventID()))
            {
                Success = Hider.FireEvent(E.StringyEvent);
            }
            if (Success
                && GameObject.Validate(ref Hider)
                && Hider.WantEvent(E.GetID(), E.GetCascadeLevel()))
            {
                Success = Hider.HandleEvent(E);
            }
            return E;
        }

        
    }
}

