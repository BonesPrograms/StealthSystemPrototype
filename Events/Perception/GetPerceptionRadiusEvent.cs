using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Capabilities.Stealth;

using static StealthSystemPrototype.Capabilities.Stealth.BasePerception;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionRadiusEvent : IPerceptionEvent<GetPerceptionRadiusEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Name;

        public Type Type;

        public PerceptionSense Sense;

        public Radius BaseRadius;

        public Radius Radius;

        public GetPerceptionRadiusEvent()
            : base()
        {
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseRadius = default;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseRadius = default;
        }

        public static GetPerceptionRadiusEvent FromPool<T>(
            GameObject Perceiver,
            T Perception,
            Radius BaseRadius)
            where T : BasePerception
        {
            if (Perception != null
                && FromPool(Perceiver) is GetPerceptionRadiusEvent E)
            {
                E.Name = Perception.GetType().Name;
                E.Type = Perception.GetType();
                E.Sense = Perception.Sense;
                E.BaseRadius = BaseRadius;
                E.Radius = BaseRadius;
            }
            return null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameter(nameof(Name), Name)
                ?.SetParameter(nameof(Type), Type)
                ?.SetParameter(nameof(Sense), Sense)
                ?.SetParameter(nameof(BaseRadius), BaseRadius)
                ?.SetParameter(nameof(Radius), Radius)
                ;

        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            Radius = StringyEvent?.GetParameter<Radius>(nameof(Radius)) ?? Radius;
        }

        public static Radius GetFor<T>(
            GameObject Perceiver,
            T Perception,
            Radius BaseRadius)
            where T : BasePerception
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool(
                    Perceiver: Perceiver,
                    Perception: Perception,
                    BaseRadius: BaseRadius) is not GetPerceptionRadiusEvent E)
                return null;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);
            
            if (proceed)
                E.UpdateFromStringyEvent();

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            return proceed
                ? E.GetRadius()
                : null;
        }

        private static void SetClamp(
            ref Radius Radius,
            Range BaseClamp,
            int? Min = null,
            int? Max = null)
            => Radius = new(
                Source: Radius,
                Clamp: (Min ?? BaseClamp.Start.Value)..(Max ?? BaseClamp.End.Value));

        private GetPerceptionRadiusEvent SetRadius(int? Min = null, int? Max = null)
        {
            SetClamp(ref Radius, SCORE_CLAMP, Min, Max);
            return this;
        }

        public GetPerceptionRadiusEvent SetMinRadius(int MinRadius)
            => SetRadius(MinRadius, null);

        public GetPerceptionRadiusEvent SetRadius(int Radius)
        {
            this.Radius = new(Radius, this.Radius);
            return this;
        }
        public GetPerceptionRadiusEvent AdjustRadius(int Amount)
        {
            Radius = Radius.AdjustBy(Amount);
            return this;
        }
        public GetPerceptionRadiusEvent SetMaxRadius(int MaxScore)
            => SetRadius(null, MaxScore);

        public Radius GetRadius()
            => new(Radius, SCORE_CLAMP);
    }
}

