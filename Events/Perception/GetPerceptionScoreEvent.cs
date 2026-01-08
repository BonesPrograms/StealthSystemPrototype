using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using static StealthSystemPrototype.Capabilities.Stealth.Perception2;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionScoreEvent : IPerceptionEvent<GetPerceptionScoreEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public string Type;

        public int BaseScore;

        public int Score;

        public int BaseRadius;

        public int Radius;

        public GetPerceptionScoreEvent()
            : base()
        {
            Type = null;
            BaseScore = 0;
            BaseRadius = 0;
        }

        public override void Reset()
        {
            base.Reset();
            Type = null;
            BaseScore = 0;
            BaseRadius = 0;
        }

        public static GetPerceptionScoreEvent FromPool(
            GameObject Perceiver,
            string Type,
            int BaseScore,
            int Score,
            int BaseRadius,
            int Radius)
        {
            if (FromPool(Perceiver) is GetPerceptionScoreEvent E)
            {
                E.Type = Type;
                E.BaseScore = BaseScore;
                E.Score = Score;
                E.BaseRadius = BaseRadius;
                E.Radius = Radius;
            }
            return null;
        }

        public static PerceptionScore GetFor(
            GameObject Perceiver,
            string Type,
            int BaseScore,
            int Score,
            int BaseRadius,
            int Radius)
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool(
                    Perceiver: Perceiver, 
                    Type: Type, 
                    BaseScore: BaseScore, 
                    Score: Score, 
                    BaseRadius: BaseRadius, 
                    Radius: Radius) is not GetPerceptionScoreEvent E)
                return PerceptionScore.Empty;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);

            E.UpdateFromStringyEvent(ClearStringyAfter: true);

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            return proceed ? new(Type, Score, Radius) : PerceptionScore.Empty;
        }

        public GetPerceptionScoreEvent SetMinScore(int Score)
        {
            if (this.Score < Score)
                this.Score = Score.Restrain(PerceptionScore.MIN_PERCEPTION_VALUE, PerceptionScore.MAX_PERCEPTION_VALUE);
            return this;
        }
        public GetPerceptionScoreEvent SetScore(int Score)
        {
            this.Score = Score.Restrain(PerceptionScore.MIN_PERCEPTION_VALUE, PerceptionScore.MAX_PERCEPTION_VALUE);
            return this;
        }
        public GetPerceptionScoreEvent AdjustScore(int Amount)
        {
            Score = + (Score + Amount).Restrain(PerceptionScore.MIN_PERCEPTION_VALUE, PerceptionScore.MAX_PERCEPTION_VALUE);
            return this;
        }
        public GetPerceptionScoreEvent SetMaxScore(int Score)
        {
            if (this.Score > Score)
                this.Score = Score.Restrain(PerceptionScore.MIN_PERCEPTION_VALUE, PerceptionScore.MAX_PERCEPTION_VALUE);
            return this;
        }

        public GetPerceptionScoreEvent SetMinRadius(int Radius)
        {
            if (this.Radius < Radius)
                this.Radius = Radius.Restrain(PerceptionScore.MIN_PERCEPTION_RADIUS, PerceptionScore.MAX_PERCEPTION_RADIUS);
            return this;
        }
        public GetPerceptionScoreEvent SetRadius(int Radius)
        {
            this.Radius = Radius.Restrain(PerceptionScore.MIN_PERCEPTION_RADIUS, PerceptionScore.MAX_PERCEPTION_RADIUS);
            return this;
        }
        public GetPerceptionScoreEvent AdjustRadius(int Amount)
        {
            Radius = (Radius + Amount).Restrain(PerceptionScore.MIN_PERCEPTION_RADIUS, PerceptionScore.MAX_PERCEPTION_RADIUS);
            return this;
        }
        public GetPerceptionScoreEvent SetMaxRadius(int Radius)
        {
            if (this.Radius > Radius)
                this.Radius = Radius.Restrain(PerceptionScore.MIN_PERCEPTION_RADIUS, PerceptionScore.MAX_PERCEPTION_RADIUS);
            return this;
        }
    }
}

