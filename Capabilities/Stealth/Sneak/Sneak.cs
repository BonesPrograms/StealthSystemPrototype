using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Events;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
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
            BaseConcealedAction ConcealedAction)
        {
            ConcealedAction.Configure();
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
    }
}
