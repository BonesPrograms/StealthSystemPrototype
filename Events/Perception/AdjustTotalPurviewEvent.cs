using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Capabilities.Stealth.Perception.IPurview;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class AdjustTotalPurviewEvent : IPerceptionEvent<AdjustTotalPurviewEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Name;

        public Type Type;

        public Type PurviewType;

        protected int BaseValue;

        protected int LinearAdjustment;

        protected double PercentAdjustment;

        protected int Min;

        protected int Max;

        public AdjustTotalPurviewEvent()
            : base()
        {
            Name = null;
            Type = null;
            PurviewType = null;
            BaseValue = 0;
            LinearAdjustment = 0;
            PercentAdjustment = 0.0;
            Min = 0;
            Max = 0;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            PurviewType = null;
            BaseValue = 0;
            LinearAdjustment = 0;
            PercentAdjustment = 0.0;
            Min = 0;
            Max = 0;
        }

        public static AdjustTotalPurviewEvent FromPool(
            GameObject Perceiver,
            BasePerception Perception,
            BasePurview Purview,
            int BaseValue)
        {
            if (Perception == null
                || FromPool(Perceiver) is not AdjustTotalPurviewEvent E)
                return null;

            E.Name = Perception.Name;
            E.Type = Perception.GetType();
            E.PurviewType = Purview.GetType();
            E.BaseValue = BaseValue;
            E.Min = MIN_VALUE;
            E.Max = MAX_VALUE;
            E.GetStringyEvent();
            return E;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameterOrNullExisting(nameof(Name), Name)
                ?.SetParameterOrNullExisting(nameof(Type), Type)
                ?.SetParameterOrNullExisting(nameof(PurviewType), PurviewType)
                ?.SetParameterOrNullExisting(nameof(BaseValue), BaseValue)
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
            BasePerception Perception,
            BasePurview Purview,
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
                    Purview: Purview,
                    BaseValue: BaseValue) is not AdjustTotalPurviewEvent E)
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

        public int GetBaseValue()
            => BaseValue;

        public int GetLinearAdjustment()
            => LinearAdjustment;

        public double GetPercentAdjustment()
            => PercentAdjustment;

        public AdjustTotalPurviewEvent SetMinValue(int Min)
        {
            this.Min = Min.Clamp(MIN_VALUE, MAX_VALUE);
            Max = Max.Clamp(Min, MAX_VALUE);
            return this;
        }
        public AdjustTotalPurviewEvent AdjustByAmount(int Amount)
        {
            LinearAdjustment += Amount;
            return this;
        }
        public AdjustTotalPurviewEvent AdjustByFactor(double Factor)
        {
            PercentAdjustment += Factor;
            return this;
        }
        public AdjustTotalPurviewEvent SetMaxValue(int Max)
        {
            this.Max = Max.Clamp(MIN_VALUE, MAX_VALUE);
            Min = Min.Clamp(MIN_VALUE, Max);
            return this;
        }

        protected int CalculateAdjustment()
            => BaseValue - (int)Math.Ceiling(BaseValue * (1.0 + LinearAdjustment) + LinearAdjustment);

        protected int GetFinalValue()
            => BaseValue + CalculateAdjustment();

        public int GetAdjustment()
        {
            int adjustment = CalculateAdjustment();
            if (GetFinalValue() > MAX_VALUE)
                adjustment += MAX_VALUE - GetFinalValue();
            else
            if (GetFinalValue() < MIN_VALUE)
                adjustment += MIN_VALUE - GetFinalValue();
            return adjustment;
        }
    }
}

