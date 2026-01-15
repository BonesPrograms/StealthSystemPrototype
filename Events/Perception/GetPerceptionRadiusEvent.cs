using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Capabilities.Stealth;

using static StealthSystemPrototype.Utils;
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
            BaseRadius = null;
            Radius = null;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseRadius = null;
            Radius = null;
        }

        public static GetPerceptionRadiusEvent FromPool<T>(
            GameObject Perceiver,
            T Perception,
            Radius BaseRadius)
            where T : BasePerception
        {
            if (Perception == null
                || FromPool(Perceiver) is not GetPerceptionRadiusEvent E)
                return null;

            E.Name = Perception.Name;
            E.Type = Perception.GetType();
            E.Sense = Perception.Sense;
            E.BaseRadius = BaseRadius;
            E.Radius = BaseRadius;
            E.StringyEvent = E.GetStringyEvent();
            return E;
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

            if (StringyEvent?.GetParameter(nameof(BaseRadius)) is Radius baseRadius)
                BaseRadius = baseRadius;

            if (StringyEvent?.GetParameter(nameof(Radius)) is Radius radius)
                Radius = radius;
        }

        public static Radius GetFor<T>(
            GameObject Perceiver,
            T Perception,
            Radius BaseRadius)
            where T : BasePerception
        {
            UnityEngine.Debug.Log(
                CallChain(nameof(GetPerceptionRadiusEvent), nameof(GetFor)) + "(" +
                nameof(Perceiver) + ": " + (Perceiver?.DebugName ?? "no one") + ", " +
                Perception.ToString());

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


            UnityEngine.Debug.Log(" ".ThisManyTimes(4) +
                nameof(E.BaseRadius) + ": " + (E.BaseRadius?.ToString() ?? "none") + ", " +
                nameof(E.Radius) + ": " + (E.Radius?.ToString() ?? "none"));

            return E.GetRadius();
        }

        private static void SetClamp(
            ref Radius Radius,
            InclusiveRange BaseClamp,
            int? Min = null,
            int? Max = null)
        {
            int start = Min ?? BaseClamp.Min;
            int length = start + (Max ?? BaseClamp.Max);
            Radius.SetClamp(new InclusiveRange(start, length).Clamp(BaseClamp));
        }

        private GetPerceptionRadiusEvent SetClamp(int? Min = null, int? Max = null)
        {
            SetClamp(ref Radius, RADIUS_CLAMP, Min, Max);
            return this;
        }

        public GetPerceptionRadiusEvent SetMinRadius(int Min)
            => SetClamp(Min, null);

        public GetPerceptionRadiusEvent SetRadius(int Radius)
        {
            this.Radius.SetValue(Radius);
            return this;
        }
        public GetPerceptionRadiusEvent AdjustRadius(int Amount)
        {
            Radius.AdjustBy(Amount);
            return this;
        }
        public GetPerceptionRadiusEvent SetMaxRadius(int Max)
            => SetClamp(null, Max);

        public Radius GetRadius()
            => Radius = new Radius(Radius, RADIUS_CLAMP).SetValue(Radius.GetValue());
    }
}

