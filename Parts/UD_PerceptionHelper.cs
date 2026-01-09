using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events;

using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using static StealthSystemPrototype.Capabilities.Stealth.Perception2;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_PerceptionHelper 
        : IScribedPart
        , IModEventHandler<GetPerceptionsEvent>
        , IModEventHandler<GetPerceptionRatingEvent>
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

        public UD_PerceptionHelper()
        {
        }

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == GetPerceptionsEvent.ID
            || ID == GetPerceptionRatingEvent.ID
            ;

        public bool HandleEvent(GetPerceptionsEvent E)
        {
            E.AddPerception(new Visual(ParentObject));
            E.AddPerception(new Auditory(ParentObject));
            E.AddPerception(new Olfactory(ParentObject));
            return base.HandleEvent(E);
        }

        public bool HandleEvent(GetPerceptionRatingEvent E)
        {
            if (E.Type == PerceptionScore.VISIUAL
                && ParentObject.RequirePart<Mutations>() is var mutations
                && mutations.MutationList.Any(bm => VisionMutations.Contains(bm.GetDisplayName())))
            {
                E.SetMinScore(40);
                E.SetMinRadius(E.BaseRadius + 2);
            }
            if (E.Type == PerceptionScore.OLFACTORY
                && ParentObject.GetBlueprint().InheritsFrom("Animal"))
            {
                E.SetMinScore(40);
                E.SetMinRadius(E.BaseRadius + 2);
            }
            if (E.Type == PerceptionScore.OLFACTORY
                && ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
            {
                E.AdjustScore(2 * heightenedSmell.Level);
                E.SetMinRadius(E.BaseRadius + 2 * heightenedSmell.Level);
            }
            if (E.Type.EqualsAny(
                new string[]
                {
                    PerceptionScore.VISIUAL,
                    PerceptionScore.AUDITORY,
                    PerceptionScore.OLFACTORY,
                }))
            {
                if (ParentObject.Body is Body body
                    && body.LoopPart("Face", bp => !bp.IsDismembered) is List<BodyPart> facesList)
                {
                    if (facesList.Count > 1)
                    {
                        E.AdjustScore(15);
                        E.SetMinRadius(E.Radius + 2);
                    }
                    else
                    if (facesList.Count < 1)
                    {
                        E.SetMaxScore(0);
                        E.SetMaxRadius(0);
                    }
                }
            }
            return base.HandleEvent(E);
        }
    }
}
