using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

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

        public static List<GameObject> GetFor(GameObject Hider, ref List<GameObject> Witnesses)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Hider?.DebugName ?? "null"),
                });

            Witnesses ??= Event.NewGameObjectList();

            if (!GameObject.Validate(ref Hider)
                || FromPool(Hider, Witnesses) is not GetWitnessesEvent E)
                return Witnesses;

            Witnesses = E.Witnesses;

            if (Hider.GetBlueprint() is GameObjectBlueprint hiderModel)
            {
                if (hiderModel.HasPart(nameof(UD_PerceptionHelper)))
                    Hider.RequirePerceptions();

                if (hiderModel.HasPart(nameof(UD_Witness)))
                    Hider.RequirePart<UD_Witness>();
            }

            bool proceed = true;
            if (proceed)
                proceed = Hider.GetCurrentZone().FireEvent(E.StringyEvent);

            if (proceed)
                E.UpdateFromStringyEvent();

            if (proceed)
                proceed = Hider.GetCurrentZone().HandleEvent(E);

            if (!proceed)
                Witnesses.Clear();

            return Witnesses;
        }

        public GetWitnessesEvent AddWitness(GameObject Witness)
        {
            Witnesses ??= Event.NewGameObjectList();
            if (GameObject.Validate(ref Witness))
                Witnesses.AddIfNot(Witness, go => Witnesses.Contains(go));
            return this;
        }

        public GetWitnessesEvent AddWitness(IComponent<GameObject> WitnessComponent)
            => AddWitness(WitnessComponent?.GetComponentBasis());

        public GetWitnessesEvent AddWitness(BasePerception Perception)
            => AddWitness(Perception?.Owner);
    }
}

