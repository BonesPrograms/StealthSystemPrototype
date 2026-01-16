using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

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

        public static PerceptionRack GetFor(GameObject Perceiver, PerceptionRack Perceptions)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Perceiver)
                || FromPool(Perceiver, Perceptions ?? new PerceptionRack(Perceiver)) is not GetPerceptionsEvent E)
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
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                    Debug.Arg(Perception?.ToString()),
                });

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
            ClampedDieRoll BaseScore,
            Radius BaseRadius,
            bool Override = true)
            where T : IPart, new()
            => AddPerception(
                Perception: new IPartPerception<T>(
                    Source: IPart,
                    Sense: Sense, 
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius), 
                Override: Override);

        public GetPerceptionsEvent AddIPartPerception<T>(
            T IPart,
            PerceptionSense Sense,
            bool Override = true)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: Sense, 
                BaseScore: BasePerception.BASE_DIE_ROLL, 
                BaseRadius: BasePerception.BASE_RADIUS, 
                Override: Override);

        public GetPerceptionsEvent AddVisualIPartPerception<T>(
            T IPart,
            bool Override = true)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Visual, 
                BaseScore: BasePerception.BASE_DIE_ROLL, 
                BaseRadius: new(BasePerception.BASE_RADIUS, BasePerception.VisualFlag), 
                Override: Override);

        public GetPerceptionsEvent AddAuditoryIPartPerception<T>(
            T IPart,
            bool Override = true)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Auditory, 
                BaseScore: BasePerception.BASE_DIE_ROLL, 
                BaseRadius: new(BasePerception.BASE_RADIUS, BasePerception.AuditoryFlag), 
                Override: Override);

        public GetPerceptionsEvent AddOlfactoryIPartPerception<T>(
            T IPart,
            bool Override = true)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Olfactory, 
                BaseScore: BasePerception.BASE_DIE_ROLL, 
                BaseRadius: new(BasePerception.BASE_RADIUS, BasePerception.OlfactoryFlag), 
                Override: Override);

        public GetPerceptionsEvent AddPsionicIPartPerception<T>(
            T IPart,
            bool Override = true)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Psionic, 
                BaseScore: BasePerception.BASE_DIE_ROLL, 
                BaseRadius: new(BasePerception.BASE_RADIUS, BasePerception.PsionicFlag), 
                Override: Override);

        public GetPerceptionsEvent AddBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            ClampedDieRoll BaseScore,
            Radius BaseRadius,
            bool Override = true)
            => AddPerception(
                Perception: new BodyPartPerception(
                    Source: BodyPart,
                    Sense: Sense, 
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius), 
                Override: Override);

        public GetPerceptionsEvent AddBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            bool Override = true)
            => AddBodyPartPerception(
                BodyPart: BodyPart,
                Sense: Sense, 
                BaseScore: BasePerception.BASE_DIE_ROLL, 
                BaseRadius: BasePerception.BASE_RADIUS, 
                Override: Override);
    }
}

