using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetSneakPerformanceEvent : ISneakEvent<GetSneakPerformanceEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetSneakPerformanceEvent()
            : base()
        {
        }

        public static SneakPerformance GetFor(GameObject Hider, ref SneakPerformance Performance)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            if (!GameObject.Validate(ref Hider)
                || Process(
                    Hider: Hider,
                    Performance: ref Performance, 
                    Success: out bool success) is not GetSneakPerformanceEvent E
                || !success)
                return null;
;
            if (success)
                success = E.Witnesses.FireEvent(E.StringyEvent, true);

            if (success)
                E.UpdateFromStringyEvent();

            if (success)
                success = E.Witnesses.HandleEvent(E, true);

            E.Performance.WantsSync = false;
            return E.Performance;
        }

        public GetSneakPerformanceEvent SetClamp(string SenseName, InclusiveRange Clamp)
        {
            Performance.SetClamp(SenseName, Clamp);
            return this;
        }
        public GetSneakPerformanceEvent SetMin(string SenseName, int Min)
        {
            Performance.SetMin(SenseName, Min);
            return this;
        }
        public GetSneakPerformanceEvent SetRating(string SenseName, int Rating)
        {
            Performance.SetRating(SenseName, Rating);
            return this;
        }
        public GetSneakPerformanceEvent AdjustRating(string SenseName, int Amount)
        {
            Performance.AdjustRating(SenseName, Amount);
            return this;
        }
        public GetSneakPerformanceEvent SetMax(string SenseName, int Max)
        {
            Performance.SetMax(SenseName, Max);
            return this;
        }

        public GetSneakPerformanceEvent AdjustMoveSpeedMultiplier<T>(T Source, int Value, string SourceDisplay = null)
            where T : IComponent<GameObject>, new()
        {
            Performance.AdjustMoveSpeedMultiplier(Source, Value, SourceDisplay);
            return this;
        }

        public GetSneakPerformanceEvent AdjustQuicknessMultiplier<T>(T Source, int Value, string SourceDisplay = null)
            where T : IComponent<GameObject>, new()
        {
            Performance.AdjustQuicknessMultiplier(Source, Value, SourceDisplay);
            return this;
        }
    }
}

