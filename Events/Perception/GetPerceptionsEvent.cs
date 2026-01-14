using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using StealthSystemPrototype.Capabilities.Stealth;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionsEvent : IPerceptionEvent<GetPerceptionsEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetPerceptionsEvent()
            : base()
        {
        }

        public static Perceptions GetFor(GameObject Perceiver)
        {
            if (!GameObject.Validate(ref Perceiver)
                || FromPool(Perceiver, new Perceptions(Perceiver)) is not GetPerceptionsEvent E)
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
                    && !E.Perceptions.IsNullOrEmpty() 
                ? E.Perceptions
                : null;
        }

        public GetPerceptionsEvent AddPerception<T>(
            T Perception,
            bool Override = true)
            where T
            : BasePerception
            , new()
        {
            UnityEngine.Debug.Log(
                (nameof(AddPerception) + "(" + 
                    Perceiver?.DebugName?.Strip() ?? "no one") + ", " + 
                    nameof(Perception) + ": " + (typeof(T)?.ToStringWithGenerics() ?? "none??") +
                ")");

            Perceptions ??= new(Perceiver);
            if (Perception != null)
            {
                if (Perception.Owner != Perceiver)
                    Perception.Owner = Perceiver;
                Perceptions.Add(Perception, Override);
            }
            return this;
        }

        public GetPerceptionsEvent AddIPartPerception<T>(
            T IPart,
            PerceptionSense Sense,
            ClampedInclusiveRange BaseScore,
            Radius BaseRadius,
            bool Override = true)
            where T : IPart, new()
            => AddPerception(
                Perception: new IPartPerception<T>(
                    Source: IPart,
                    Sense: Sense, 
                    BaseScore: BaseScore, 
                    BaseRadius: BaseRadius), 
                Override: Override);

        public GetPerceptionsEvent AddIPartPerception<T>(T IPart,
            PerceptionSense Sense,
            bool Override = true)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: Sense, 
                BaseScore: BasePerception.BASE_SCORE, 
                BaseRadius: BasePerception.BASE_RADIUS, 
                Override: Override);

        public GetPerceptionsEvent AddBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            ClampedInclusiveRange BaseScore,
            Radius BaseRadius,
            bool Override = true)
            => AddPerception(
                Perception: new BodyPartPerception(
                    Source: BodyPart,
                    Sense: Sense, 
                    BaseScore: BaseScore, 
                    BaseRadius: BaseRadius), 
                Override: Override);

        public GetPerceptionsEvent AddBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            bool Override = true)
            => AddBodyPartPerception(
                BodyPart: BodyPart,
                Sense: Sense, 
                BaseScore: BasePerception.BASE_SCORE, 
                BaseRadius: BasePerception.BASE_RADIUS, 
                Override: Override);
    }
}

