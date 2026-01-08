using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events.Perception;

using static StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace XRL.World.Parts
{
    [Serializable]
    public class Bones_PerceptionHelper : IScribedPart, IModEventHandler<GetPerceptionTypesEvent>
    {
        public static List<string> VisionMutations => MutationFactory.AllMutationEntries()
            ?.Aggregate(
                seed: new List<string>(),
                func: delegate (List<string> Accumulator, MutationEntry Next)
                {
                    if (Next.GetDisplayName() is string displayName
                        && displayName.ContainsAnyNoCase("Vision", "Sight"))
                        Accumulator.Add(displayName);
                    return Accumulator;
                })
            ?.ToList();

        public Bones_PerceptionHelper()
        {
        }

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == GetPerceptionTypesEvent.ID
            ;

        public bool HandleEvent(GetPerceptionTypesEvent E)
        {
            E.AddBaseScore(PerceptionScore.VISIUAL);
            E.AddBaseScore(PerceptionScore.AUDITORY);
            E.AddBaseScore(PerceptionScore.OLFACTORY);
            return base.HandleEvent(E);
        }
    }
}
