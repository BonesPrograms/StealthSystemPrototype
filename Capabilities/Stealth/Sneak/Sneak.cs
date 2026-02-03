using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.World;
using XRL.World.Parts;
using XRL.World.AI;
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

        public static ISneakSource GetSneakSource(GameObject Sneaker)
        {
            List<ISneakSource> sneakSources = new();
            if (Sneaker.PartsList
                .Where(p => typeof(ISneakSource).IsAssignableFrom(p.GetType()))
                .Select(p => p as ISneakSource).ToList() is List<ISneakSource> iPartSneakSources)
                sneakSources.AddRange(iPartSneakSources);

            if (Sneaker.Effects
                .Where(p => typeof(ISneakSource).IsAssignableFrom(p.GetType()))
                .Select(p => p as ISneakSource).ToList() is List<ISneakSource> fxSneakSources)
                sneakSources.AddRange(fxSneakSources);

            sneakSources.OrderInPlace((x, y) => -x.BaseSneakPerformance.CompareTo(y.BaseSneakPerformance));

            if (sneakSources.IsNullOrEmpty())
                return null;

            return sneakSources[0];
        }

        public static bool IsBeingPerceived(GameObject Hider)
            => GetSneakSource(Hider) is ISneakSource sneakSource
            && sneakSource.IsBeingPerceived;

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
                return true
                    .PopupBeforeReturn(("=subject.Refname= is " + (IsBeingPerceived(player) ? null : "not ") + "being perceived.").StartReplace().AddObject(player).ToString());

            string messageStart = "=subject.Refname= is being perceived by:\n".StartReplace().AddObject(player).ToString();
            string nooneString = "no one!";

            using IsPerceivingSpec isPerceiving = new(player);

            string stringGameObjectDetectionOpinionLevel(GameObject Perceiver)
                => StringGameObjectDetectionOpinionLevel(Perceiver, player);

            string perceiverList = player.CurrentZone
                .GetObjects(isPerceiving)
                .Select(stringGameObjectDetectionOpinionLevel)
                .Aggregate("", NewLineDelimitedAggregator);

            return true
                .PopupBeforeReturn(messageStart + (!perceiverList.IsNullOrEmpty() ? perceiverList : nooneString));
        }

        #endregion
    }
}
