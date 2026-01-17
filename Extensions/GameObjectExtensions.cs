using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.World;
using XRL.World.AI;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.Rules;

using Range = System.Range;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Alerts;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype
{
    public static class GameObjectExtensions
    {
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Extensions),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    //{ nameof(SetMin), false },
                });

        public static PerceptionRack GetPerceptions(this GameObject Object)
            => Object.GetPart<UD_PerceptionHelper>()?.Perceptions;

        public static PerceptionRack RequirePerceptions(this GameObject Object)
            => Object.RequirePart<UD_PerceptionHelper>()?.Perceptions;

        public static bool HasPerception<T>(this GameObject Object, T Item = null)
            where T : IPerception, new()
            => Object.RequirePerceptions().Has(Item);

        public static bool HasPerception(this GameObject Object, string Name)
            => Object
                ?.RequirePerceptions()
                ?.Has(Name)
            ?? false;

        public static T GetPerception<T>(this GameObject Object)
            where T : IPerception, new()
            => Object.RequirePerceptions()?.Get<T>();

        public static IPerception GetPerception(this GameObject Object, string Name)
            => Object.RequirePerceptions().Get(Name);

        public static IPerception GetFirstPerceptionOfSense(this GameObject Object, PerceptionSense Sense)
            => Object.RequirePerceptions().GetFirstOfSense(Sense);

        public static bool TryGetPerception<T>(this GameObject Object, out T Item)
            where T : IPerception, new()
            => Object.TryGetPerception(out Item);

        public static IPerception AddPerception<T>(
            this GameObject Object,
            T Perception,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where T : IPerception, new()
        {
            Object.RequirePerceptions()?.Add(Perception, DoRegistration, Initial, Creation);
            return Perception;
        }

        public static IPerception AddPerception<T>(
            this GameObject Object,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPerception, new()
            => Object.RequirePerceptions()?.Add<T>(DoRegistration, Creation);

        public static IPerception RequirePerception<T>(
            this GameObject Object,
            bool Creation = false)
            where T : IPerception, new()
            => Object.RequirePerceptions()?.Require<T>(Creation);
    }
}
