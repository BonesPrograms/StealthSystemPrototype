using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;
using XRL.World.Parts;

using static StealthSystemPrototype.Capabilities.Stealth.BasePerception;
using static StealthSystemPrototype.Capabilities.Stealth.Perception2;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionRatingEvent : IPerceptionEvent<GetPerceptionRatingEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Type;

        public int BaseScore;

        public int Score;

        public int BaseRadius;

        public int Radius;

        private Range ScoreClamp;

        private Range RadiusClamp;

        public GetPerceptionRatingEvent()
            : base()
        {
            Type = null;
            BaseScore = 0;
            BaseRadius = 0;
            ScoreClamp = default;
            RadiusClamp = default;
        }

        public override void Reset()
        {
            base.Reset();
            Type = null;
            BaseScore = 0;
            BaseRadius = 0;
        }

        public static GetPerceptionRatingEvent FromPool<T>(
            GameObject Perceiver,
            int BaseScore,
            int Score,
            int BaseRadius,
            int Radius)
            where T : BasePerception, new()
        {
            if (FromPool(Perceiver) is GetPerceptionRatingEvent E)
            {
                E.Type = typeof(T)?.Name;
                E.BaseScore = BaseScore;
                E.Score = Score;
                E.BaseRadius = BaseRadius;
                E.Radius = Radius;
                E.ScoreClamp = BasePerception.PERCEPTION_SCORE_CLAMP;
                E.RadiusClamp = BasePerception.PERCEPTION_RADIUS_CLAMP;
            }
            return null;
        }

        public static PerceptionRating? GetFor<T>(
            GameObject Perceiver,
            int BaseScore,
            int Score,
            int BaseRadius,
            int Radius)
            where T : BasePerception, new()
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool<T>(
                    Perceiver: Perceiver, 
                    BaseScore: BaseScore, 
                    Score: Score, 
                    BaseRadius: BaseRadius, 
                    Radius: Radius) is not GetPerceptionRatingEvent E)
                return null;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);

            E.UpdateFromStringyEvent(ClearStringyAfter: true);

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            return proceed
                ? new PerceptionRating(E.GetScore(), E.GetRadius())
                : null;
        }

        private static void SetClamp(ref Range Restraint, Range BaseRestraint, int? Min = null, int? Max = null)
            => Restraint = new((Min ?? Restraint.Start.Value).Clamp(BaseRestraint), (Max ?? Restraint.End.Value).Clamp(BaseRestraint));

        private GetPerceptionRatingEvent SetScoreClamp(int? Min = null, int? Max = null)
        {
            SetClamp(ref ScoreClamp, BasePerception.PERCEPTION_SCORE_CLAMP, Min, Max);
            return this;
        }
        private GetPerceptionRatingEvent SetRadiusClamp(int? Min = null, int? Max = null)
        {
            SetClamp(ref RadiusClamp, BasePerception.PERCEPTION_RADIUS_CLAMP, Min, Max);
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

