using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth;

using XRL.World;
using XRL.World.Parts;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetWitnessesEvent : IWitnessEvent<GetWitnessesEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        public GetWitnessesEvent()
            : base()
        {
        }

        public static List<GameObject> GetFor(GameObject Hider, List<GameObject> Witnesses = null)
        {
            if (!GameObject.Validate(ref Hider)
                || FromPool(Hider, Witnesses) is not GetWitnessesEvent E)
                return null;

            bool proceed = true;
            if (proceed)
                proceed = Hider.GetCurrentZone().FireEvent(E.StringyEvent);

            if (proceed)
                E.UpdateFromStringyEvent();

            if (proceed)
                proceed = Hider.GetCurrentZone().HandleEvent(E);

            return proceed
                ? E.Witnesses
                : null;
        }

        public GetWitnessesEvent AddWitness(GameObject Witness)
        {
            Witnesses ??= Event.NewGameObjectList();
            if (GameObject.Validate(ref Witness))
                Witnesses.AddIfNot(Witness, go => Witnesses.Contains(go));
            return this;
        }

        public GetWitnessesEvent AddWitness(IPart WitnessPart)
            => AddWitness(WitnessPart?.ParentObject);

        public GetWitnessesEvent AddWitness(BasePerception Perception)
            => AddWitness(Perception?.Owner);
    }
}

