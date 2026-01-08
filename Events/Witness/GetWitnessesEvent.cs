using System;
using System.Collections.Generic;
using System.Text;

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
                || FromPool(Hider) is not GetWitnessesEvent E)
                return null;

            bool proceed = true;
            if (proceed)
                proceed = Hider.GetCurrentZone().FireEvent(E.StringyEvent);

            E.UpdateFromStringyEvent(ClearStringyAfter: true);

            if (proceed)
                proceed = Hider.GetCurrentZone().HandleEvent(E);

            return E.Witnesses;
        }

        public GetWitnessesEvent AddWitness(GameObject Witness)
        {
            Witnesses ??= Event.NewGameObjectList();
            if (GameObject.Validate(ref Witness))
                Witnesses.Add(Witness);
            return this;
        }

        public GetWitnessesEvent AddWitness(IPart WitnessPart)
            => AddWitness(WitnessPart?.ParentObject);
    }
}

