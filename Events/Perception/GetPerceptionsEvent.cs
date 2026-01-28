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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

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

        public GetPerceptionsEvent AddPerception(
            IPerception Perception,
            bool DoRegistration = true,
            bool Creation = false)
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

        public GetPerceptionsEvent RequirePerception<T>(
            T Perception,
            bool Creation = false)
            where T : class, IPerception, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perception?.ToString()),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (!Perceptions.TryGet(out T perception))
            {
                perception = Perception;
                AddPerception(perception, DoRegistration: true, Creation);
            }
            if (perception.Owner != Perceiver)
                perception.Owner = Perceiver;
                
            return this;
        }

        public GetPerceptionsEvent AddBodyPartPerception<P, V, A>(
            BodyPart BodyPart,
            int Level,
            V Purview,
            bool DoRegistration = true,
            bool Creation = false)
            where P : class, IBodyPartPerception, IAlertTypedPerception<A, V>, new()
            where V : class, IPurview<A>, new()
            where A : class, IAlert, new()
        {
            P perception = new()
            {
                Source = BodyPart,
                Level = Level,
                Purview = Purview,
            };
            if (perception != null)
            {
                perception.Purview.SetParentPerception(perception);
                return AddPerception(
                    Perception: perception,
                    DoRegistration: DoRegistration,
                    Creation: Creation);
            }
            return this;
        }

        public GetPerceptionsEvent RequireBodyPartPerception<P>(
            BodyPart BodyPart,
            int Level,
            int Purview,
            bool Creation = false)
            where P : class, IBodyPartPerception, new()
        {
            P perception = new()
            {
                Source = BodyPart,
                Level = Level,
            };
            perception.AssignDefaultPurview(Purview);
            if (perception != null)
            {
                perception.Purview.SetParentPerception(perception);
                return RequirePerception(
                    Perception: perception,
                    Creation: Creation);
            }
            return this;
        }
    }
}

