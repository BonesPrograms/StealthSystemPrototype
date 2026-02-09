using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

using XRL;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;
using XRL.World.Effects;
using XRL.Messages;
using XRL.Wish;

using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Perceptions.Specs;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [HasWishCommand]
    public static class Sneak
    {
        public static string VERBING => "sneaking";

        /// <summary>
        /// Prepares the supplied <paramref name="ConcealedAction"/> and then passes it to <see cref="TryConcealActionEvent.Send"/>.
        /// </summary>
        /// <param name="Hider">The <see cref="GameObject"/> that is attempting to conceal an action.</param>
        /// <param name="Performance">The <see cref="SneakPerformance"/> of the <paramref name="Hider"/>.</param>
        /// <param name="ConcealedAction">The <see cref="BaseConcealedAction"/> on which to perform preparations and call <see cref="TryConcealActionEvent.Send"/>.</param>
        /// <returns><see langword="true"/> if the preparation performed doesn't empty the contents of the <see cref="BaseConcealedAction"/>;<br/><see langword="false"/> otherwise.</returns>
        public static bool TryConcealAction(
            GameObject Hider,
            SneakPerformance Performance,
            BaseConcealedAction ConcealedAction,
            GameObject AlertObject = null,
            Cell AlertCell = null)
        {
            ConcealedAction.SetHider(Hider)
                .SetAlertObject(AlertObject)
                .SetAlertLocation(AlertCell)
                .Configure();
            GetActionAlertsEvent.GetFor(ConcealedAction);
            if (!ConcealedAction.IsNullOrEmpty())
            {
                TryConcealActionEvent.Send(Hider, Performance, ConcealedAction);
                return true;
            }
            return false;
        }

        public static bool IsBeingPerceived(GameObject Hider)
            => SneakSource(Hider) is ISneakSource sneakSource
            && sneakSource.SneakBeingPerceived;

        private static ISneakSource SneakSource(GameObject Source, Predicate<ISneakSource> Filter)
            => Source?.GetFirstSneakSource(Filter);

        private static ISneakSource SneakSource(GameObject Sneaker)
            => SneakSource(Sneaker, null);

        private static IEnumerable<ISneakSource> SneakSources(GameObject Sneaker, Predicate<ISneakSource> Filter)
            => Sneaker.GetSneakSources(Filter);

        private static IEnumerable<ISneakSource> SneakSources(GameObject Sneaker)
            => SneakSources(Sneaker, null);

        private static bool TryGetSneakSource(GameObject Source, ref ISneakSource SS)
            => (SS ??= SneakSource(Source)) != null;

        private static bool HasSource(UD_Sneaking FX)
            => FX.Source != null;

        private static bool HasActivatedAbilityID(ISneakSource SS)
            => SS.SneakActivatedAbilityID != Guid.Empty;

        public static bool AbilitySetup(GameObject Source, GameObject Object, ISneakSource SS = null)
        {
            if ((SS ??= SneakSource(Source)) == null)
                return false;

            string abilityName = SS.SneakActivatedAbilityName ?? "Sneak";

            if (SS.SneakSourceDescription != null)
                abilityName += " (" + SS.SneakSourceDescription + ")";

            SS.SneakActivatedAbilityID = Object.AddActivatedAbility(
                Name: abilityName,
                SS.SneakActivatedAbilityCommand,
                SS.SneakActivatedAbilityClass,
                Description: null, // explicitly null. This should be defined in the xmls.
                Icon: "\u0001",
                Toggleable: true,
                DefaultToggleState: SS.SneakSneaking,
                ActiveToggle: true,
                IsRealityDistortionBased: SS.SneakActivatedAbilityIsRealityDistortionBased,
                CommandForDescription: "Command_UD_SneakToggle");

            return true;
        }

        public static bool AbilityTeardown(GameObject Source, GameObject Object, ISneakSource SS = null)
        {
            if (TryGetSneakSource(Source, ref SS))
            {
                Guid ID = SS.SneakActivatedAbilityID;
                Object.RemoveActivatedAbility(ref ID);
                SS.SneakActivatedAbilityID = ID;
            }
            return true;
        }

        public static UD_Sneaking GetSneakingEffectFromSource(GameObject Source, GameObject Object)
        {
            if (GameObject.Validate(ref Source) && GameObject.Validate(ref Object))
                foreach (UD_Sneaking fx in Object.YieldEffects<UD_Sneaking>())
                    if (fx is UD_Sneaking sneaking
                        && sneaking.Source == Source)
                        return sneaking;
            return null;
        }

        public static bool IsSneaking(GameObject Object)
            => Object?.IsSneaking() ?? false;

        public static bool StartSneaking(GameObject Source, GameObject Object, ISneakSource SS = null)
        {
            if (!TryGetSneakSource(Source, ref SS))
                return false;

            if (SS.SneakSneaking)
                return false;

            if (!Object.CheckFrozen())
                return false;

            if (!Object.CanChangeMovementMode(VERBING.Capitalize(), ShowMessage: true))
                return false;

            string abortedByEventMessage = null;

            if (!BeforeSneakEvent.Check(Object, SS.SneakPerformance, SS.SneakWitnesses, ref abortedByEventMessage))
                return !abortedByEventMessage.IsNullOrEmpty()
                    && Object.ShowFailure(abortedByEventMessage);

            Object.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_wings_fly_move");
            if (Object.GetEffectCount(typeof(UD_Sneaking)) == 0)
            {
                if (Object.IsVisible()) // this should mention vanishing if it's a non-player who the player can no longer detect.
                    ("=subject.T= =subject.verb:begin= " + VERBING + "!")
                        .StartReplace()
                        .AddObject(Object)
                        .EmitMessage();

                Object.MovementModeChanged("Sneaking");
            }
            else
            if (Object.IsPlayer())
                ("=subject.T= =subject.verb:begin= employing an additional means of " + VERBING + "!")
                    .StartReplace()
                    .AddObject(Object)
                    .EmitMessage();

            SS.SneakSneaking = true;
            Object.ApplyEffect(new UD_Sneaking(Source));
            Object.ToggleActivatedAbility(SS.SneakActivatedAbilityID);
            Object.FireEvent("SneakStarted");
            ObjectStartedSneakingEvent.Send(Object, SS.SneakPerformance, SS.SneakWitnesses);
            return true;
        }

        public static bool StopSneaking(GameObject Source, GameObject Object, ISneakSource SS = null, bool Silent = false, bool FromFail = false)
        {
            if (!TryGetSneakSource(Source, ref SS))
                return false;

            if (!SS.SneakSneaking)
                return false;

            if (Object == null)
            {
                SS.SneakSneaking = false;
                return false;
            }

            int effectCount = Object.GetEffectCount(typeof(UD_Sneaking));
            Object.GetCurrentCell();
            if (!Silent)
            {
                if (effectCount <= 1)
                {
                    if (Object.IsVisible()) // this should mention appearing if it's a non-player who the player can now detect.
                        ("=subject.T= =subject.verb:stop= " + VERBING + ".")
                            .StartReplace()
                            .AddObject(Object)
                            .EmitMessage();
                }
                else
                if (Object.IsPlayer())
                    ("=subject.T= =subject.verb:cease= employing one of your means of " + VERBING + ".")
                        .StartReplace()
                        .AddObject(Object)
                        .EmitMessage();
            }

            SS.SneakSneaking = false;
            if (!Object.RemoveEffect(typeof(UD_Sneaking), FX => (FX as UD_Sneaking).Source == Source))
                Object.RemoveEffect<UD_Sneaking>();

            Object.ToggleActivatedAbility(SS.SneakActivatedAbilityID, Silent: false);
            Object.FireEvent(nameof(UD_Sneaking) + "StoppedFromOneSource");
            if (effectCount <= 1)
            {
                Object.FireEvent(nameof(UD_Sneaking) + "Stopped");
                if (!FromFail)
                {
                    NoLongerSneaking(Object);
                    Object.MovementModeChanged("Not" + nameof(UD_Sneaking));
                }
                ObjectStoppedSneakingEvent.Send(Object, SS.SneakPerformance, SS.SneakWitnesses);
            }
            return true;
        }
        private static void NoLongerSneaking(GameObject Object)
        {
            // do clean-up here.
        }

        public static SneakPerformance GetBestSneakPerformance(GameObject Object)
        {

        }

        #region Wishes

        private static string StringGameObjectDetectionOpinionLevel(GameObject Perceiver, GameObject Hider)
            => "=subject.Refname= is "
                    .StartReplace()
                    .AddObject(Perceiver)
                    .ToString()
            + (Perceiver.GetOpinionDetectionsFor(Hider, null)
                    .FirstOrDefault()
                    ?.Level
                    .ToStringWithNum()
                ?? "NO_AWARENESS_LEVEL");

        [WishCommand(Command = "SSP_Sneak is player perceived")]
        public static bool Player_IsBeingPerceived_WishHandler()
            => Player_IsBeingPerceived_WishHandler(null);

        [WishCommand(Command = "SSP_Sneak is player perceived")]
        public static bool Player_IsBeingPerceived_WishHandler(string flags)
        {
            if (The.Player is not GameObject player)
                return false
                    .PopupBeforeReturn("No player? How'd you make this wish???");

            if (flags.IsNullOrEmpty()
                || !flags.ContainsAnyNoCase(
                    Strings: new string[]
                    {
                        "verbose",
                        "-v",
                    }))
            {
                string beingPerceived = (IsBeingPerceived(player) ? null : "not ") + "being perceived.";
                string perceivedMsg = ("=subject.Refname= is " + beingPerceived)
                    .StartReplace()
                    .AddObject(player)
                    .ToString();

                return true
                    .PopupBeforeReturn(perceivedMsg);
            }

            string messageStart = "=subject.Refname= is being perceived by:\n".StartReplace().AddObject(player).ToString();
            string nooneString = "no one!";

            using IsPerceivingSpec isPerceivingPlayer = new(player);

            string stringGameObjectDetectionOpinionLevel(GameObject Perceiver)
                => StringGameObjectDetectionOpinionLevel(Perceiver, player);

            string perceiverList = player.CurrentZone
                .GetObjects(isPerceivingPlayer)
                .Select(stringGameObjectDetectionOpinionLevel)
                .Aggregate("", NewLineDelimitedAggregator);

            return true
                .PopupBeforeReturn(messageStart + (!perceiverList.IsNullOrEmpty() ? perceiverList : nooneString));
        }

        #endregion
    }
}
