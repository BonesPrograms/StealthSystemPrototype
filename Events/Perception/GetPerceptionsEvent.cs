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
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetPerceptionsEvent : IPerceptionEvent<GetPerceptionsEvent>
    {
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEachFalse(
                Type: typeof(StealthSystemPrototype.Events.GetPerceptionsEvent),
                Methods: new string[]
                {
                    nameof(AddPerception),
                });
        #endregion

        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetPerceptionsEvent()
            : base()
        {
        }

        public static void GetFor(GameObject Perceiver, ref PerceptionRack Perceptions)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.DebugName ?? "null"),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (!GameObject.Validate(ref Perceiver)
                || FromPool(Perceiver, Perceptions) is not GetPerceptionsEvent E)
                return;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);

            if (proceed)
                E.UpdateFromStringyEvent();

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            if (!proceed)
                Perceptions.Clear();
        }

        public GetPerceptionsEvent AddPerception<TSense>(
            IPerception<TSense> Perception,
            bool DoRegistration = true,
            bool Creation = false)
            where TSense : ISense<TSense>, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perceiver?.MiniDebugName() ?? "null"),
                    Debug.Arg(Perception?.ToString()),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (Perception.Owner != Perceiver)
                Perception.Owner = Perceiver;

            Perceptions.Add(Perception, DoRegistration, Creation);

            return this;
        }

        public GetPerceptionsEvent RequirePerception<TSense>(
            IPerception<TSense> Perception = null,
            bool Creation = false)
            where TSense : ISense<TSense>, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perception?.ToString()),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (!Perceptions.TryGet(out IPerception<TSense> perception))
            {
                perception = Perception;
                AddPerception(perception, DoRegistration: true, Creation);
            }
            if (perception.Owner != Perceiver)
                perception.Owner = Perceiver;
                
            return this;
        }

        public GetPerceptionsEvent AddIPartPerception<T, TSense>(
            T IPart,
            ClampedDieRoll BaseScore,
            Purview BaseRadius,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            where TSense : ISense<TSense>, new()
            => AddPerception(
                Perception: new IPartPerception<T, TSense>(
                    Source: IPart,
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireIPartPerception<T, TSense>(
            T IPart,
            ClampedDieRoll BaseScore,
            Purview BaseRadius,
            bool Creation = false)
            where T : IPart, new()
            where TSense : ISense<TSense>, new()
            => RequirePerception(
                Perception: new IPartPerception<T, TSense>(
                    Source: IPart,
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                Creation: Creation);

        public GetPerceptionsEvent AddIPartPerception<T, TSense>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            where TSense : ISense<TSense>, new()
            => AddIPartPerception<T, TSense>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireIPartPerception<T, TSense>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            where TSense : ISense<TSense>, new()
            => RequireIPartPerception<T, TSense>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                Creation: Creation);

        public GetPerceptionsEvent AddVisualIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception<T, Visual>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.VisualFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireVisualIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception<T, Visual>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.VisualFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddAuditoryIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception<T, Auditory>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.AuditoryFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireAuditoryIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception<T, Auditory>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.AuditoryFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddOlfactoryIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception<T, Olfactory>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.OlfactoryFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireOlfactoryIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception<T, Olfactory>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.OlfactoryFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddPsionicIPartPerception<T>(
            T IPart,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPart, new()
            => AddIPartPerception<T, Psionic>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.PsionicFlag),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequirePsionicIPartPerception<T>(
            T IPart,
            bool Creation = false)
            where T : IPart, new()
            => RequireIPartPerception<T, Psionic>(
                IPart: IPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: new(IPerception.BASE_RADIUS, IPerception.PsionicFlag),
                Creation: Creation);

        public GetPerceptionsEvent AddBodyPartPerception<TSense>(
            BodyPart BodyPart,
            ClampedDieRoll BaseScore,
            Purview BaseRadius,
            bool DoRegistration = true,
            bool Creation = false)
            where TSense : ISense<TSense>, new()
            => AddPerception(
                Perception: new BodyPartPerception<TSense>(
                    Source: BodyPart,
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireBodyPartPerception<TSense>(
            BodyPart BodyPart,
            ClampedDieRoll BaseScore,
            Purview BaseRadius,
            bool Creation = false)
            where TSense : ISense<TSense>, new()
            => RequirePerception(
                Perception: new BodyPartPerception<TSense>(
                    Source: BodyPart,
                    BaseDieRoll: BaseScore, 
                    BaseRadius: BaseRadius),
                Creation: Creation);

        public GetPerceptionsEvent AddBodyPartPerception<TSense>(
            BodyPart BodyPart,
            bool DoRegistration = true,
            bool Creation = false)
            where TSense : ISense<TSense>, new()
            => AddBodyPartPerception<TSense>(
                BodyPart: BodyPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                DoRegistration: DoRegistration,
                Creation: Creation);

        public GetPerceptionsEvent RequireBodyPartPerception<TSense>(
            BodyPart BodyPart,
            bool Creation = false)
            where TSense : ISense<TSense>, new()
            => RequireBodyPartPerception<TSense>(
                BodyPart: BodyPart,
                BaseScore: IPerception.BASE_DIE_ROLL, 
                BaseRadius: IPerception.BASE_RADIUS,
                Creation: Creation);
    }
}

