using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Capabilities.Stealth;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Capabilities.Stealth.BasePerception;
using XRL.Rules;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionDieRollEvent : IPerceptionEvent<GetPerceptionDieRollEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Name;

        public Type Type;

        public PerceptionSense Sense;

        public ClampedDieRoll BaseDieRoll;

        public ClampedDieRoll DieRoll;

        public GetPerceptionDieRollEvent()
            : base()
        {
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseDieRoll = null;
            DieRoll = null;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseDieRoll = null;
            DieRoll = null;
        }

        public static GetPerceptionDieRollEvent FromPool<T>(
            GameObject Perceiver,
            T Perception,
            ClampedDieRoll BaseDieRoll)
            where T : BasePerception
        {
            if (Perception == null
                || FromPool(Perceiver) is not GetPerceptionDieRollEvent E)
                return null;

            E.Name = Perception.Name;
            E.Type = Perception.GetType();
            E.Sense = Perception.Sense;
            E.BaseDieRoll = BaseDieRoll;
            E.DieRoll = BaseDieRoll;
            E.StringyEvent = E.GetStringyEvent();
            return E;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameter(nameof(Name), Name)
                ?.SetParameter(nameof(Type), Type)
                ?.SetParameter(nameof(Sense), Sense)
                ?.SetParameter(nameof(BaseDieRoll), BaseDieRoll)
                ?.SetParameter(nameof(DieRoll), DieRoll)
                ;

        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            if (StringyEvent?.GetParameter(nameof(BaseDieRoll)) is ClampedDieRoll baseDieRoll)
                BaseDieRoll = baseDieRoll;

            if (StringyEvent?.GetParameter(nameof(DieRoll)) is ClampedDieRoll dieRoll)
                DieRoll = dieRoll;
        }

        public static ClampedDieRoll GetFor<T>(
            GameObject Perceiver,
            T Perception,
            ClampedDieRoll BaseDieRoll)
            where T : BasePerception
        {
            UnityEngine.Debug.Log(
                CallChain(nameof(GetPerceptionDieRollEvent), nameof(GetFor)) + "(" +
                nameof(Perceiver) + ": " + (Perceiver?.DebugName ?? "no one") + ", " +
                Perception.ToString());

            if (!GameObject.Validate(ref Perceiver)
                || FromPool(
                    Perceiver: Perceiver,
                    Perception: Perception,
                    BaseDieRoll: BaseDieRoll) is not GetPerceptionDieRollEvent E)
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

            return E.GetDieRoll();
        }

        private static void SetDieRollClamp(
            ClampedDieRoll DieRoll,
            InclusiveRange BaseClamp,
            int? Min = null,
            int? Max = null)
        {
            int start = Min ?? BaseClamp.Min;
            int length = start + (Max ?? BaseClamp.Max);
            DieRoll.SetClamp(new InclusiveRange(start, length).Clamp(BaseClamp));
        }

        private GetPerceptionDieRollEvent SetDieRollClamp(int? Min = null, int? Max = null)
        {
            SetDieRollClamp(DieRoll, DIE_ROLL_CLAMP, Min, Max);
            return this;
        }

        public GetPerceptionDieRollEvent SetDieRollMin(int Min)
            => SetDieRollClamp(Min, null);

        public GetPerceptionDieRollEvent SetDieRoll(DieRoll DieRoll)
        {
            this.DieRoll.SetDieRoll(DieRoll);
            return this;
        }
        public GetPerceptionDieRollEvent AdjustDieRoll(int Amount)
        {
            DieRoll.AdjustBy(Amount);
            return this;
        }
        public GetPerceptionDieRollEvent SetDieRollMax(int Max)
            => SetDieRollClamp(null, Max);

        public ClampedDieRoll GetDieRoll()
            => DieRoll = new(DieRoll, DIE_ROLL_CLAMP);
    }
}

