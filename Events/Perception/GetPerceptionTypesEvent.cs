using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using static StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Events.Perception
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionTypesEvent : IPerceptionEvent<GetPerceptionTypesEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetPerceptionTypesEvent()
            : base()
        {
        }

        public static Dictionary<string, PerceptionScore> GetFor(GameObject Perceiver)
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool(Perceiver) is not GetPerceptionTypesEvent E)
                return null;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);

            E.UpdateFromStringyEvent(ClearStringyAfter: true);

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            return E.PerceptionScores;
        }

        private GetPerceptionTypesEvent AddPerceptionScore(string Type, PerceptionScore PerceptionScore, bool Override = true)
        {
            PerceptionScores ??= new();
            if (Override || !PerceptionScores.ContainsKey(Type))
                PerceptionScores[Type] = PerceptionScore;
            return this;
        }

        public virtual GetPerceptionTypesEvent AddScore(PerceptionScore PerceptionScore, bool Override = true)
            => AddPerceptionScore(PerceptionScore.Type, PerceptionScore, Override);

        public virtual GetPerceptionTypesEvent AddBaseScore(string Type, bool Override = true)
            => AddPerceptionScore(Type, new PerceptionScore(Type, PerceptionScore.BASE_PERCEPTION_VALUE, PerceptionScore.BASE_PERCEPTION_RADIUS), Override);
    }
}

