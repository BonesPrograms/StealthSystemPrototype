using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using static StealthSystemPrototype.Capabilities.Stealth.Perception2;

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
                || FromPool(Perceiver) is not GetPerceptionsEvent E)
                return null;

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
                proceed = Perceiver.FireEvent(E.StringyEvent);

            E.UpdateFromStringyEvent(ClearStringyAfter: true);

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
                proceed = Perceiver.HandleEvent(E);

            return E.Perceptions;
        }

        public GetPerceptionsEvent AddPerception<T>(T Perception, bool Override = true)
            where T : BasePerception, new()
        {
            Perceptions ??= new();
            Perceptions.Add(Perception, Override);
            return this;
        }

        public GetPerceptionsEvent AddIPartPerception<T>(T IPart, PerceptionSense Sense, int BaseScore, int BaseRadius, bool Override = true)
            where T : IPart, new()
            => AddPerception(
                Perception: new IPartPerception<T>(
                    Owner: IPart?.ParentObject, 
                    Source: IPart,
                    Sense: Sense, 
                    Score: BaseScore, 
                    Radius: BaseRadius), 
                Override: Override);

        public GetPerceptionsEvent AddBodyPartPerception(BodyPart BodyPart, PerceptionSense Sense, int BaseScore, int BaseRadius, bool Override = true)
            => AddPerception(
                Perception: new BodyPartPerception(
                    Owner: BodyPart?.ParentBody?.ParentObject, 
                    Source: BodyPart,
                    Sense: Sense, 
                    Score: BaseScore, 
                    Radius: BaseRadius), 
                Override: Override);
    }
}

