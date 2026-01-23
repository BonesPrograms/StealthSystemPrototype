using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;
using XRL.Rules;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Perceptions.IPerception;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class AdjustTotalPerceptionLevelEvent : IPerceptionEvent<AdjustTotalPerceptionLevelEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Name;

        public Type Type;

        protected int BaseLevel;

        protected int LinearAdjustment;

        protected double PercentAdjustment;

        public int Min;

        public int Max;

        public AdjustTotalPerceptionLevelEvent()
            : base()
        {
            Name = null;
            Type = null;
            BaseLevel = 0;
            LinearAdjustment = 0;
            PercentAdjustment = 0.0;
            Min = MIN_LEVEL;
            Max = MAX_LEVEL;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            BaseLevel = 0;
            LinearAdjustment = 0;
            PercentAdjustment = 0.0;
            Min = MIN_LEVEL;
            Max = MAX_LEVEL;
        }

        public static AdjustTotalPerceptionLevelEvent FromPool(
            GameObject Perceiver,
            IPerception Perception,
            int BaseValue)
        {
            if (Perception == null
                || FromPool(Perceiver, Perception) is not AdjustTotalPerceptionLevelEvent E)
                return null;

            E.Name = Perception.Name;
            E.Type = Perception.GetType();
            E.BaseLevel = BaseValue;
            E.Min = MIN_LEVEL;
            E.Max = MAX_LEVEL;
            E.GetStringyEvent();
            return E;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameterOrNullExisting(nameof(Name), Name)
                ?.SetParameterOrNullExisting(nameof(Type), Type)
                ?.SetParameterOrNullExisting(nameof(BaseLevel), BaseLevel)
                ?.SetParameterOrNullExisting(nameof(LinearAdjustment), LinearAdjustment)
                ?.SetParameterOrNullExisting(nameof(PercentAdjustment), PercentAdjustment)
                ?.SetParameterOrNullExisting(nameof(Min), Min)
                ?.SetParameterOrNullExisting(nameof(Max), Max)
                ;

        public override void UpdateFromStringyEvent()
        {
            if (StringyEvent?.GetIntParameter(nameof(LinearAdjustment)) is int linearAdjustment)
                LinearAdjustment = linearAdjustment;

            if (StringyEvent?.GetParameter(nameof(PercentAdjustment)) is double percentAdjustment)
                PercentAdjustment = percentAdjustment;
        }

        public static int GetFor(
            GameObject Perceiver,
            IPerception Perception,
            int BaseValue)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                    Debug.Arg(Perception.ToString()),
                });

            if (!GameObject.Validate(ref Perceiver)
                || FromPool(
                    Perceiver: Perceiver,
                    Perception: Perception,
                    BaseValue: BaseValue) is not AdjustTotalPerceptionLevelEvent E)
                return 0;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);
            
            if (proceed)
                E.UpdateFromStringyEvent();

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            return E.GetAdjustment();
        }

        public int GetBaseLevel()
            => BaseLevel;

        public int GetLinearAdjustment()
            => LinearAdjustment;

        public double GetPercentAdjustment()
            => PercentAdjustment;

        public AdjustTotalPerceptionLevelEvent SetMinValue(int Min)
        {
            this.Min = Min.Clamp(MIN_LEVEL, MAX_LEVEL);
            return this;
        }
        public AdjustTotalPerceptionLevelEvent AdjustByAmount(int Amount)
        {
            LinearAdjustment += Amount;
            return this;
        }
        public AdjustTotalPerceptionLevelEvent AdjustByFactor(double Factor)
        {
            PercentAdjustment += Factor;
            return this;
        }
        public AdjustTotalPerceptionLevelEvent SetMaxValue(int Max)
        {
            this.Max = Max.Clamp(MIN_LEVEL, MAX_LEVEL);
            return this;
        }

        protected int CalculateAdjustment()
            => BaseLevel - (int)Math.Ceiling(BaseLevel * (1.0 + LinearAdjustment) + LinearAdjustment);

        protected int GetFinalLevel()
            => BaseLevel + CalculateAdjustment();

        public int GetAdjustment()
        {
            int adjustment = CalculateAdjustment();
            if (GetFinalLevel() > MAX_LEVEL)
                adjustment += MAX_LEVEL - GetFinalLevel();
            else
            if (GetFinalLevel() < MIN_LEVEL)
                adjustment += MIN_LEVEL - GetFinalLevel();
            return adjustment;
        }
    }
}

