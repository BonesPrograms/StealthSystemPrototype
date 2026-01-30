using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

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
                    // nameof(AddPerception),
                    nameof(doDebugRegistry),
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

            if (FromPool(Perceiver, Perceptions) is not GetPerceptionsEvent E)
                return;

            Debug.CheckYeh("Got Event", Indent: indent[1]);

            bool proceed = true;
            if (proceed
                && Perceiver.HasRegisteredEvent(E.GetRegisteredEventID()))
            {
                proceed = Perceiver.FireEvent(E.StringyEvent);
                Debug.YehNah(nameof(Perceiver.HasRegisteredEvent), proceed, Indent: indent[1]);
            }

            if (proceed)
            {
                E.UpdateFromStringyEvent();
                Debug.YehNah(nameof(UpdateFromStringyEvent), proceed, Indent: indent[1]);
            }

            if (proceed
                && Perceiver.WantEvent(E.GetID(), E.GetCascadeLevel()))
            {
                proceed = Perceiver.HandleEvent(E);
                Debug.YehNah(nameof(Perceiver.WantEvent), proceed, Indent: indent[1]);
            }

            if (!proceed)
                Perceptions.Clear();

            Debug.YehNah(Utils.CallChain(nameof(GetPerceptionsEvent), nameof(GetFor)), proceed, Indent: indent[0]);
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

        public GetPerceptionsEvent RequirePerception<P>(
            P Perception,
            bool Creation = false)
            where P : class, IPerception, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Perception?.ToString()),
                });

            Perceptions ??= new PerceptionRack(Perceiver);

            if (!Perceptions.TryGet(out P perception))
            {
                perception = Perception;
                AddPerception(perception, DoRegistration: true, Creation);
            }
            if (perception.Owner != Perceiver)
                perception.Owner = Perceiver;
                
            return this;
        }

        public GetPerceptionsEvent RequireBodyPartPerception<P>(
            BodyPart BodyPart,
            int Level,
            int PurviewValue,
            bool Creation = false)
            where P : class, IBodyPartPerception, new()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(BodyPart?.ToString()),
                    Debug.Arg(nameof(Level), Level),
                    Debug.Arg(nameof(PurviewValue), PurviewValue),
                });

            P perception = new()
            {
                Source = BodyPart,
                Level = Level,
            };
            if (perception != null)
            {
                Debug.CheckYeh(perception?.ToString(), Indent: indent[1]);

                perception.ConfigurePurview(PurviewValue);

                Debug.CheckYeh("Configured.", Indent: indent[1]);

                return RequirePerception(
                    Perception: perception,
                    Creation: Creation);
            }
            return this;
        }
    }
}

