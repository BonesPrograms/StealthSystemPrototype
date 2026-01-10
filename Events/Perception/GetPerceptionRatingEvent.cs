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
    public class GetPerceptionRatingEvent : IPerceptionEvent<GetPerceptionRatingEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Name;

        public Type Type;

        public PerceptionSense Sense;

        public int BaseScore;

        public int Score;

        public int BaseRadius;

        public int Radius;

        private Range ScoreClamp;

        private Range RadiusClamp;

        public GetPerceptionRatingEvent()
            : base()
        {
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseScore = 0;
            BaseRadius = 0;
            ScoreClamp = default;
            RadiusClamp = default;
        }

        public override void Reset()
        {
            base.Reset();
            Name = null;
            Type = null;
            Sense = PerceptionSense.None;
            BaseScore = 0;
            BaseRadius = 0;
        }

        public static GetPerceptionRatingEvent FromPool<T>(
            GameObject Perceiver,
            T Perception,
            int BaseScore,
            int BaseRadius)
            where T : BasePerception
        {
            if (Perception != null
                && FromPool(Perceiver) is GetPerceptionRatingEvent E)
            {
                E.Name = Perception.GetType().Name;
                E.Type = Perception.GetType();
                E.Sense = Perception.Sense;
                E.BaseScore = BaseScore;
                E.Score = BaseScore;
                E.BaseRadius = BaseRadius;
                E.Radius = BaseRadius;
                E.ScoreClamp = PERCEPTION_SCORE_CLAMP;
                E.RadiusClamp = PERCEPTION_RADIUS_CLAMP;
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
                ?.SetParameter(nameof(BaseRadius), BaseRadius)
                ?.SetParameter(nameof(Radius), Radius)
                ?.SetParameter(nameof(ScoreClamp), ScoreClamp)
                ?.SetParameter(nameof(RadiusClamp), RadiusClamp)
                ;

        public override void UpdateFromStringyEvent()
        {
            base.UpdateFromStringyEvent();

            Score = StringyEvent?.GetIntParameter(nameof(Score)) ?? Score;

            Radius = StringyEvent?.GetIntParameter(nameof(Score)) ?? Radius;


        }

        public static PerceptionRating? GetFor<T>(
            GameObject Perceiver,
            T Perception,
            int BaseScore,
            int BaseRadius)
            where T : BasePerception
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool(
                    Perceiver: Perceiver,
                    Perception: Perception,
                    BaseScore: BaseScore, 
                    BaseRadius: BaseRadius) is not GetPerceptionRatingEvent E)
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
                ? new PerceptionRating(E.GetScore(), E.GetRadius())
                : null;
        }
        public static PerceptionRating? GetFor<T>(
            GameObject Perceiver,
            T Perception,
            PerceptionRating? Rating)
            where T : BasePerception
            => GetFor(
                Perceiver: Perceiver,
                Perception: Perception,
                BaseScore: Rating?.Score ?? BASE_PERCEPTION_SCORE,
                BaseRadius: Rating?.Radius?? BASE_PERCEPTION_RADIUS);

        private static void SetClamp(
            ref Range Restraint,
            Range BaseRestraint,
            int? Min = null,
            int? Max = null)
            => Restraint = new(
                start: (Min ?? Restraint.Start.Value).Clamp(BaseRestraint),
                end: (Max ?? Restraint.End.Value).Clamp(BaseRestraint));

        private GetPerceptionRatingEvent SetScoreClamp(int? Min = null, int? Max = null)
        {
            SetClamp(ref ScoreClamp, PERCEPTION_SCORE_CLAMP, Min, Max);
            return this;
        }
        private GetPerceptionRatingEvent SetRadiusClamp(int? Min = null, int? Max = null)
        {
            SetClamp(ref RadiusClamp, PERCEPTION_RADIUS_CLAMP, Min, Max);
            return this;
        }

        public GetPerceptionRatingEvent SetMinScore(int MinScore)
            => SetScoreClamp(MinScore, null);

        public GetPerceptionRatingEvent SetScore(int Score)
        {
            this.Score = Score;
            return this;
        }
        public GetPerceptionRatingEvent AdjustScore(int Amount)
            => SetScore(Score + Amount);

        public GetPerceptionRatingEvent SetMaxScore(int MaxScore)
            => SetScoreClamp(null, MaxScore);

        public GetPerceptionRatingEvent SetMinRadius(int MinRadius)
            => SetRadiusClamp(MinRadius, null);

        public GetPerceptionRatingEvent SetRadius(int Radius)
        {
            this.Radius = Radius;
            return this;
        }
        public GetPerceptionRatingEvent AdjustRadius(int Amount)
            => SetRadius(Score + Amount);

        public GetPerceptionRatingEvent SetMaxRadius(int MaxRadius)
            => SetRadiusClamp(null, MaxRadius);

        public int GetScore()
            => Score.Clamp(ScoreClamp);

        public int GetRadius()
            => Radius.Clamp(RadiusClamp);
    }
}

