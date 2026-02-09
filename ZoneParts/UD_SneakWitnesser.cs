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
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Perceptions.Specs;

using static StealthSystemPrototype.Utils;

namespace XRL.World.ZoneParts
{
    public class UD_SneakWitnesser : IZonePart, ISneakEventHandler
    {
        public Witnesses Witnesses;

        public Sneakers Sneakers;

        public UD_SneakWitnesser()
            : base()
        {
            Witnesses = new(this);
            Sneakers = new(this);
        }

        public bool AddSneaker(ISneakSource SneakSource)
            => SneakSource != null && Sneakers.Add(SneakSource);

        public bool RemoveSneaker(ISneakSource SneakSource)
            => SneakSource != null && Sneakers.Remove(SneakSource);

        public bool AddWitness(GameObject Witness)
            => Witness != null && Witnesses.Add(Witness);

        public bool RemoveWitness(GameObject Witness)
            => Witness != null && Witnesses.Remove(Witness);

        public bool ProcessObjectEntering(GameObject Object)
            => AddWitness(Object)
            && (Sneak.GetBestSneakPerformance(Object) is not ISneakSource sneaker
                || AddSneaker(sneaker));

        public bool ProcessObjectLeaving(GameObject Object)
            => RemoveWitness(Object)
            && (Sneak.GetBestSneakPerformance(Object) is not ISneakSource sneaker
                || RemoveSneaker(sneaker));

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == EnteringZoneEvent.ID
            || ID == ZoneThawedEvent.ID
            || ID == SuspendingEvent.ID
            ;
        public override bool HandleEvent(EnteringZoneEvent E)
        {
            if (E.Cell is Cell destination
                && destination.ParentZone == ParentZone)
            {
                ProcessObjectEntering(E.Object);
            }
            else
            if (E.Origin?.ParentZone is Zone originZone
                && originZone.TryGetPart(out UD_SneakWitnesser originSneakWitnesser))
            {
                originSneakWitnesser.ProcessObjectLeaving(E.Object);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ZoneThawedEvent E)
        {
            // Reinitialize here, probably send some events.
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(SuspendingEvent E)
        {
            Sneakers.Clear();
            Witnesses.Clear();
            return base.HandleEvent(E);
        }
    }
}
