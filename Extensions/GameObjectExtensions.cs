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
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Detetection.Opinions;
using System.Diagnostics.CodeAnalysis;
using XRL.World.Effects;

namespace StealthSystemPrototype
{
    public static class GameObjectExtensions
    {
        #region Debug Registry
        [UD_DebugRegistry]
        public static void GameObjectExtensions_DoDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.GameObjectExtensions),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(GetPerceptions), false },
                });
        #endregion
        #region Perceptions

        public static Capabilities.Stealth.PerceptionsSet GetPerceptions(this GameObject Object)
            => Object.GetPart<UD_PerceptionHelper>()?.Perceptions;

        public static Capabilities.Stealth.PerceptionsSet RequirePerceptions(this GameObject Object)
            => Object.RequirePart<UD_PerceptionHelper>()?.Perceptions;

        public static bool HasPerceptions([NotNullWhen(true)] this GameObject Object)
            => Object?.HasPart<UD_PerceptionHelper>() ?? false;

        public static bool HasAnyPerceptions([NotNullWhen(true)] this GameObject Object)
            => Object?.GetPart<UD_PerceptionHelper>()?.Perceptions is Capabilities.Stealth.PerceptionsSet perceptions
            && perceptions.Count > 0;

        public static bool HasPerception<A>([NotNullWhen(true)] this GameObject Object, BasePerception Perception = null)
            => Object?.RequirePerceptions()?.Has(Perception) ?? false;

        public static bool HasPerception(
            [NotNullWhen(true)] this GameObject Object,
            [NotNullWhen(true)] string PerceptionName,
            bool IncludeShort = false)
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

        public static bool TryGetPerception<P>(
            [NotNullWhen(true)] this GameObject Object,
            [NotNullWhen(true)] out P Perception)
            where P : BasePerception, new()
        {
            Perception = null;
            return Object?.GetPerceptions() is Capabilities.Stealth.PerceptionsSet perceptions
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

        // re-write this. it's hacky.
        public static bool WithinAnyPurview(this GameObject Object, GameObject Perceiver)
            => Perceiver?.GetPerceptions() is Capabilities.Stealth.PerceptionsSet perceptions
            && perceptions.Any(p => Object.CurrentCell.CosmeticDistanceToCell(Perceiver.CurrentCell) >= p.Purview.EffectiveValue);

        #endregion
        #region OpinionDetections

        public static IEnumerable<IOpinionDetection> GetOpinionDetections(this GameObject Perceiver, Predicate<IOpinionDetection> Filter)
        {
            if (Perceiver.Brain is not Brain brain)
                yield break;

            if (brain.Opinions is not OpinionMap opinionsMap)
                yield break;

            foreach ((int subjectID, OpinionList subjectOpinions) in opinionsMap)
                if (subjectOpinions
                        .Where(o => o is IOpinionDetection)
                        .Select(o => o as IOpinionDetection)
                    is not IEnumerable<IOpinionDetection> opinionDetections)
                    continue;
                else
                    foreach (IOpinionDetection opinionDetection in opinionDetections)
                        if (Filter == null || Filter(opinionDetection))
                            yield return opinionDetection;
        }

        public static IEnumerable<IOpinionDetection> GetOpinionDetectionsFor(this GameObject Perceiver, GameObject Hider, Predicate<IOpinionDetection> Filter)
        {
            if (Perceiver.Brain is not Brain brain)
                yield break;

            if (GetOpinionDetections(Perceiver, o => o.AlertContext.Hider == Hider) is not IEnumerable<IOpinionDetection> opinionDetections)
                yield break;

            foreach (IOpinionDetection opinionDetection in opinionDetections)
                if (Filter == null || Filter(opinionDetection))
                    yield return opinionDetection;

            /*
            if (!brain.TryGetOpinions(Hider, out OpinionList subjectOpinions))
                yield break;

            if (subjectOpinions
                    .Where(o => o is IOpinionDetection)
                    .Select(o => o as IOpinionDetection)
                is not IEnumerable<IOpinionDetection> opinionDetections)
                yield break;

            foreach (IOpinionDetection opinionDetection in opinionDetections)
                if (Filter == null || Filter(opinionDetection))
                    yield return opinionDetection;
            */
        }

        #endregion
        #region Predicates

        public static bool IsPerceiving(this GameObject Perceiver, GameObject Hider)
            => Perceiver.Brain is Brain brain
            && brain.TryGetOpinions(Hider, out OpinionList opinions)
            && !opinions
                .Where(o => o is IOpinionDetection)
                .Select(o => o as IOpinionDetection)
                .All(o => o.Level < AwarenessLevel.Aware);

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

        #endregion
        #region Sneak

        public static IEnumerable<ISneakSource> GetSneakSources(this GameObject Object, Predicate<ISneakSource> Filter)
        {
            foreach (ISneakSource partSneakSource in Object.GetPartsDescendedFrom(Filter))
                yield return partSneakSource;

            foreach (ISneakSource effectSneakSource in Object.GetEffectsDescendedFrom(Filter))
                yield return effectSneakSource;

            foreach (ISneakSource perceptionSneakSource in Object.GetPerceptionsDescendedFrom(Filter))
                yield return perceptionSneakSource;
        }

        public static IEnumerable<ISneakSource> GetSneakSources(this GameObject Object)
            => Object.GetSneakSources(null);

        public static ISneakSource GetFirstSneakSource(this GameObject Object, Predicate<ISneakSource> Filter)
            => Object.GetSneakSources(Filter).FirstOrDefault();

        public static ISneakSource GetFirstSneakSource(this GameObject Object)
            => Object.GetFirstSneakSource(null);

        public static bool IsSneaking([NotNullWhen(true)] this GameObject Object)
            => (Object?.HasEffect<UD_Sneaking>() ?? false)
            && Object.GetIntProperty("Suspend_UD_Sneak") <= 0;

        #endregion

        public static List<T> GetEffectsDescendedFrom<T>(this GameObject Object, Predicate<T> Filter)
            where T : class
            => Object.Effects
                .Where(fx => fx is T)
                .Select(fx => fx as T)
                .Where(t => Filter?.Invoke(t) ?? true)
                .ToList()
            ;
        public static List<T> GetEffectsDescendedFrom<T>(this GameObject Object)
            where T : class
            => Object.GetEffectsDescendedFrom<T>(null);

        public static List<T> GetPerceptionsDescendedFrom<T>(this GameObject Object, Predicate<T> Filter)
            where T : class
            => Object.GetPerceptions()
                .Where(p => p is T)
                .Select(p => p as T)
                .Where(t => Filter?.Invoke(t) ?? true)
                .ToList()
            ;
        public static List<T> GetPerceptionsDescendedFrom<T>(this GameObject Object)
            where T : class
            => Object.GetPerceptionsDescendedFrom<T>(null);

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
    }
}
