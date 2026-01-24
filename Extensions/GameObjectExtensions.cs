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
using StealthSystemPrototype.Detetections;

using static StealthSystemPrototype.Utils;
using XRL.UI;
using StealthSystemPrototype.Senses;
using XRL.World.Parts.Mutation;

namespace StealthSystemPrototype
{
    public static class GameObjectExtensions
    {
        #region Debug Registry
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.GameObjectExtensions),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(GetPerceptions), false },
                });
        #endregion

        public static PerceptionRack GetPerceptions(this GameObject Object)
            => Object.GetPart<UD_PerceptionHelper>()?.Perceptions;

        public static PerceptionRack RequirePerceptions(this GameObject Object)
            => Object.RequirePart<UD_PerceptionHelper>()?.Perceptions;

        public static bool HasPerception<TSense>(this GameObject Object, IPerception<TSense> Item = null)
            where TSense : ISense<TSense>, new()
            => Object.RequirePerceptions().Has(Item);

        public static bool HasPerception(this GameObject Object, string Name)
            => Object
                ?.RequirePerceptions()
                ?.Has(Name)
            ?? false;

        public static IPerception<TSense> GetPerception<TSense>(this GameObject Object)
            where TSense : ISense<TSense>, new()
            => Object.RequirePerceptions()?.Get<IPerception<TSense>, TSense>();

        public static IPerception GetPerception(this GameObject Object, string Name)
            => Object.RequirePerceptions().Get(Name);

        public static IPerception GetFirstPerceptionOfSense<TSense>(this GameObject Object, TSense Sense = null)
            where TSense : ISense<TSense>, new()
            => Object.RequirePerceptions().GetFirstOfSense<TSense>();

        public static bool TryGetPerception<TSense>(this GameObject Object, out IPerception<TSense> Item)
            where TSense : ISense<TSense>, new()
        {
            Item = null;
            return Object.GetPerceptions() is PerceptionRack perceptions
                && perceptions.TryGet(out Item);
        }

        public static IPerception AddPerception<T, TSense>(
            this GameObject Object,
            T Perception,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
        {
            Object.RequirePerceptions()?.Add(Perception, DoRegistration, Initial, Creation);
            return Perception;
        }

        public static IPerception AddPerception<T, TSense>(
            this GameObject Object,
            bool DoRegistration = true,
            bool Creation = false)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
            => Object.RequirePerceptions()?.Add<T, TSense>(DoRegistration, Creation);

        public static IPerception RequirePerception<T, TSense>(
            this GameObject Object,
            bool Creation = false)
            where T : IPerception<TSense>, new()
            where TSense : ISense<TSense>, new()
            => Object.RequirePerceptions()?.Require<T, TSense>(Creation);

        public static bool CheckNotOnWorldMap(this GameObject Object, string Verb, bool ShowMessage = false)
        {
            if (Object.OnWorldMap())
            {
                if (ShowMessage)
                    Popup.ShowFail("You cannot " + Verb + " on the world map.");
                return false;
            }
            return true;
        }

        public static void ForeachEffect<T>(
            this GameObject Object,
            Action<T> Proc)
            where T : Effect
        {
            if (Object._Effects is EffectRack effects)
                foreach (Effect effect in effects)
                    if (effect is T tEffect)
                        Proc(tEffect);
        }

        public static bool HasMentalMutations(this GameObject Object, bool RequireBaseLevels = false)
        {
            if (!Object.TryGetPart(out Mutations mutations))
                return false;

            if (mutations.ActiveMutationList is not List<BaseMutation> activeMutations
                || activeMutations.IsNullOrEmpty())
                return false;

            return RequireBaseLevels
                ? activeMutations.Any(IsMentalWithBaseLevels)
                : activeMutations.Any(bm => bm.IsMental());
        }

        public static bool EligibleForMentalMutations(this GameObject Object)
        {
            if (!Object.TryGetPart(out Mutations mutations))
                return false;

            if (mutations.HasMutation(nameof(Chimera)))
                return false;

            if (MutationFactory.AllMutationEntries()
                    ?.Where(me => me.IsMental())
                    ?.Any(me => mutations.IncludedInMutatePool(me, true))
                ?? false)
                return true;

            return Object.HasMentalMutations(true);
        }
    }
}
