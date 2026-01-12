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
    public class GetPerceptionScoreEvent : IPerceptionEvent<GetPerceptionScoreEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Name;

        public Type Type;

        public PerceptionSense Sense;

        public ClampedRange BaseScore;

        public ClampedRange Score;

        public GetPerceptionScoreEvent()
            : base()
        {
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseScore = default;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseScore = default;
        }

        public static GetPerceptionScoreEvent FromPool<T>(
            GameObject Perceiver,
            T Perception,
            ClampedRange BaseScore)
            where T : BasePerception
        {
            if (Perception != null
                && FromPool(Perceiver) is GetPerceptionScoreEvent E)
            {
                E.Name = Perception.GetType().Name;
                E.Type = Perception.GetType();
                E.Sense = Perception.Sense;
                E.BaseScore = BaseScore;
                E.Score = BaseScore;
            }
            return null;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                ?.SetParameter(nameof(Name), Name)
                ?.SetParameter(nameof(Type), Type)
                ?.SetParameter(nameof(Sense), Sense)
                ?.SetParameter(nameof(BaseScore), BaseScore)
                ?.SetParameter(nameof(Score), Score)
                ;

        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            Score = StringyEvent?.GetParameter<ClampedRange>(nameof(Score)) ?? Score;
        }

        public static ClampedRange GetFor<T>(
            GameObject Perceiver,
            T Perception,
            ClampedRange BaseScore)
            where T : BasePerception
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool(
                    Perceiver: Perceiver,
                    Perception: Perception,
                    BaseScore: BaseScore) is not GetPerceptionScoreEvent E)
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
                ? E.GetScore()
                : null;
        }

        private static void SetClamp(
            ref ClampedRange Score,
            Range BaseClamp,
            int? Min = null,
            int? Max = null)
            => Score = new(
                Value: (Min ?? BaseClamp.Start.Value)..(Max ?? BaseClamp.End.Value),
                Clamp: Score);

        private GetPerceptionScoreEvent SetScore(int? Min = null, int? Max = null)
        {
            SetClamp(ref Score, SCORE_CLAMP, Min, Max);
            return this;
        }

        public GetPerceptionScoreEvent SetMinScore(int MinScore)
            => SetScore(MinScore, null);

        public GetPerceptionScoreEvent SetScore(Range Score)
        {
            this.Score = new(Score, this.Score);
            return this;
        }
        public GetPerceptionScoreEvent AdjustScore(int Amount)
        {
            Score = Score.AdjustBy(Amount);
            return this;
        }
        public GetPerceptionScoreEvent SetMaxScore(int MaxScore)
            => SetScore(null, MaxScore);

        public ClampedRange GetScore()
            => new(Score, SCORE_CLAMP);
    }
}

