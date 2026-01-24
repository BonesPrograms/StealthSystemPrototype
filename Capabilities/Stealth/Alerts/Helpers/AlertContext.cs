using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Perceptions;

using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;
using XRL.World.Parts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Alerts
{
    public class AlertContext
    {
        public IConcealedAction ParentAction { get; protected set; }

        public IPerception Perception { get; protected set; }

        public GameObject Perceiver => Perception?.Owner;

        public IAlert Alert { get; protected set; }

        public int Intensity { get; protected set; }

        public GameObject Actor { get; protected set; }

        public Cell AlertLocation { get; protected set; }

        public IPurview Purview => Perception?.Purview;

        public bool IsWithinPurview => GetIsWinithPurview(AlertLocation);

        public SneakPerformance SneakPerformance => Actor?.GetPart<UD_Sneak>()?.SneakPerformance;

        protected AlertContext()
        {
            Perception = null;
            Alert = null;
            Intensity = 0;
            Actor = null;
            AlertLocation = null;
        }
        public AlertContext(IConcealedAction ParentAction, IPerception Perception, IAlert Alert, int Intensity, GameObject Actor, Cell AlertLocation)
            : this()
        {
            this.Perception = Perception;
            this.Alert = Alert;
            this.Intensity = Intensity;
            this.Actor = Actor;
            this.AlertLocation = AlertLocation;
        }
        public AlertContext(AlertContext Source)
            : this(Source.ParentAction, Source.Perception, Source.Alert, Source.Intensity - 1, Source.Actor, Source.AlertLocation)
        {
        }

        public bool Validate()
            => GameObject.Validate(Actor)
            && GameObject.Validate(Perceiver)
            && Perception.Validate()
            && Intensity > 0;

        public static bool Validate(ref AlertContext Context)
        {
            if (!Context.Validate())
                Context = null;

            return Context != null;
        }

        protected bool GetIsWinithPurview(Cell Cell)
            => AlertLocation != null
            && Purview != null
            && Purview.IsWithin(AlertLocation);
    }
}
