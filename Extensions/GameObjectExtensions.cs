using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.UI;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Anatomy;
using XRL.Rules;

using Range = System.Range;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

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

        public static bool HasPerception<A>(this GameObject Object, BasePerception Perception = null)
            => Object.RequirePerceptions().Has(Perception);

        public static bool HasPerception(this GameObject Object, string PerceptionName, bool IncludeShort = false)
            => Object
                ?.RequirePerceptions()
                ?.Has(PerceptionName, IncludeShort)
            ?? false;

        public static P GetPerception<P>(this GameObject Object)
            where P : BasePerception, new()
            => Object.RequirePerceptions()?.Get<P>();

        public static List<IAlertTypedPerception<A>> GetPerceptionsForAlert<A>(this GameObject Object)
            where A : BaseAlert, new()
            => Object.RequirePerceptions()?.GetForAlert<A>();

        public static IPerception GetPerception(this GameObject Object, string PerceptionName, bool IncludeShort = false)
            => Object.RequirePerceptions().Get(PerceptionName, IncludeShort);

        public static IPerception GetFirstPerceptionOfAlert<A>(this GameObject Object, A Alert = null)
            where A : class, IAlert, new()
            => Object.RequirePerceptions().GetFirstOfAlert(Alert);

        public static bool TryGetPerception<P>(this GameObject Object, out P Perception)
            where P : BasePerception, new()
        {
            Perception = null;
            return Object.GetPerceptions() is PerceptionRack perceptions
                && perceptions.TryGet(out Perception);
        }

        public static P AddPerception<P>(
            this GameObject Object,
            P Perception,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where P : BasePerception
        {
            Object.RequirePerceptions()?.Add(Perception, DoRegistration, Initial, Creation);
            return Perception;
        }

        public static P AddPerception<P>(
            this GameObject Object,
            int Level,
            int PurviewValue,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where P : BasePerception, new()
            => Object.RequirePerceptions()?.Add<P>(Level, PurviewValue, DoRegistration, Initial, Creation);

        public static P AddPerception<P>(
            this GameObject Object,
            bool DoRegistration = true,
            bool Initial = false,
            bool Creation = false)
            where P : BasePerception, new()
            => Object.RequirePerceptions()?.Add<P>(DoRegistration, Initial, Creation);

        public static P AddPerception<P>(
            this GameObject Object,
            int Level,
            int PurviewValue,
            bool DoRegistration = true,
            bool Creation = false)
            where P : BasePerception, new()
            => Object.RequirePerceptions()?.Add<P>(Level, PurviewValue, DoRegistration, Creation);

        public static P RequirePerception<P>(
            this GameObject Object,
            bool Creation = false)
            where P : BasePerception, new()
            => Object.RequirePerceptions()?.Require<P>(Creation);

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
