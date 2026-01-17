using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
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
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPerception, new()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                    Debug.Arg(Perception?.ToString()),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (Perception.Owner != Perceiver)
                Perception.Owner = Perceiver;

            Perceptions.Add(Perception, DoRegistration, Creation);

            return this;
        }

        public GetPerceptionsEvent RequirePerception<T>(
            T Perception = null,
            bool Creation = false)
            where T : IPerception, new()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                    Debug.Arg(Perception?.ToString()),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (!Perceptions.Has<T>())
                if (Perception.Owner != Perceiver)
                {
                    Perception.Owner = Perceiver;
                    AddPerception(Perception, DoRegistration: true, Creation);
                }
            return this;
        }

        public GetPerceptionsEvent AddIPartPerception<T>(
            T IPart,
            PerceptionSense Sense,
            ClampedDieRoll BaseScore,
            Radius BaseRadius,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddPerception(
                Perception: new IPartPerception<T>(
                    Source: IPart,
                    Sense: Sense, 
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireIPartPerception<T>(
            T IPart,
            PerceptionSense Sense,
            ClampedDieRoll BaseScore,
            Radius BaseRadius,
            bool Creation = false)
            where T : IPart, new()
            => RequirePerception(
                Perception: new IPartPerception<T>(
                    Source: IPart,
                    Sense: Sense, 
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                Creation: Creation);

        public GetPerceptionsEvent AddIPartPerception<T>(
            T IPart,
            PerceptionSense Sense,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: Sense, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireIPartPerception<T>(
            T IPart,
            PerceptionSense Sense,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception(
                IPart: IPart,
                Sense: Sense, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                Creation: Creation);

        public GetPerceptionsEvent AddVisualIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Visual, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.VisualFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireVisualIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Visual, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.VisualFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddAuditoryIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Auditory, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.AuditoryFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireAuditoryIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Auditory, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.AuditoryFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddOlfactoryIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Olfactory, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.OlfactoryFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireOlfactoryIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Olfactory, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.OlfactoryFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddPsionicIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Psionic, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.PsionicFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequirePsionicIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception(
                IPart: IPart,
                Sense: PerceptionSense.Psionic, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.PsionicFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            ClampedDieRoll BaseScore,
            Radius BaseRadius,
            bool DoRegistration = true,
            bool Creation = false)
            => AddPerception(
                Perception: new BodyPartPerception(
                    Source: BodyPart,
                    Sense: Sense, 
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            ClampedDieRoll BaseScore,
            Radius BaseRadius,
            bool Creation = false)
            => RequirePerception(
                Perception: new BodyPartPerception(
                    Source: BodyPart,
                    Sense: Sense, 
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                Creation: Creation);

        public GetPerceptionsEvent AddBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            bool DoRegistration = true,
            bool Creation = false)
            => AddBodyPartPerception(
                BodyPart: BodyPart,
                Sense: Sense, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireBodyPartPerception(
            BodyPart BodyPart,
            PerceptionSense Sense,
            bool Creation = false)
            => RequireBodyPartPerception(
                BodyPart: BodyPart,
                Sense: Sense, 
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                Creation: Creation);
    }
}

