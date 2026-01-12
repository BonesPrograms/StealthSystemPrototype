using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events;

using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_PerceptionHelper 
        : IScribedPart
        , IModEventHandler<GetPerceptionsEvent>
        , IModEventHandler<GetPerceptionScoreEvent>
        , IModEventHandler<GetPerceptionRadiusEvent>
    {
        public const string ANIMAL_BLUEPRINT = "Animal";

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
            || ID == GetPerceptionScoreEvent.ID
            || ID == GetPerceptionRadiusEvent.ID
            ;

        public bool HandleEvent(GetPerceptionsEvent E)
        {
            E.AddPerception(new Visual(ParentObject));
            E.AddPerception(new Auditory(ParentObject));
            E.AddPerception(new Olfactory(ParentObject));

            if (ParentObject.TryGetPart(out Esper esper))
                E.AddPerception(new EsperPsionic(esper));

            if (ParentObject.TryGetPart(out HeightenedHearing heightenedHearing))
                E.AddIPartPerception(heightenedHearing, PerceptionSense.Auditory);

            if (ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.AddIPartPerception(heightenedSmell, PerceptionSense.Olfactory);

            if (ParentObject.TryGetPart(out NightVision nightVision))
                E.AddIPartPerception(nightVision, PerceptionSense.Visual);

            if (ParentObject.TryGetPart(out DarkVision darkVision))
                E.AddIPartPerception(darkVision, PerceptionSense.Visual);

            if (ParentObject.TryGetPart(out SensePsychic sensePsychic))
                E.AddIPartPerception(sensePsychic, PerceptionSense.Psionic);

            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetPerceptionsEvent) + " -> " + 
                nameof(E.Perceptions) + " (" + (E.Perceptions?.Count ?? 0) + ")");

            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetPerceptionScoreEvent E)
        {
            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetPerceptionScoreEvent) + " -> " + 
                (E.Type?.Name ?? "no type?"));

            if (E.Sense == PerceptionSense.Visual
                && ParentObject.RequirePart<Mutations>() is var mutations
                && mutations.MutationList.Any(bm => VisionMutations.Contains(bm.GetDisplayName())))
                E.SetMinScore(40);

            if (E.Sense == PerceptionSense.Olfactory
                && ParentObject.GetBlueprint().InheritsFrom(ANIMAL_BLUEPRINT))
                E.SetMinScore(40);

            if (E.Sense == PerceptionSense.Olfactory
                && ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.AdjustScore(2 * heightenedSmell.Level);

            if (E.Sense.EqualsAny(
                new PerceptionSense[]
                {
                    PerceptionSense.Visual,
                    PerceptionSense.Auditory,
                    PerceptionSense.Olfactory,
                })
                && E.Type.InheritsFrom(typeof(SimplePerception))
                && ParentObject.Body is Body body
                && body.LoopPart(SimplePerception.FACE_BODYPART, bp => !bp.IsDismembered) is List<BodyPart> facesList)
            {
                if (facesList.Count > 1)
                    E.AdjustScore(15);
                else
                if (facesList.Count < 1)
                    E.SetMaxScore(0);
            }

            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetPerceptionRadiusEvent E)
        {
            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetPerceptionRadiusEvent) + " -> " + 
                (E.Type?.Name ?? "no type?"));

            if (E.Sense == PerceptionSense.Visual
                && ParentObject.RequirePart<Mutations>() is var mutations
                && mutations.MutationList.Any(bm => VisionMutations.Contains(bm.GetDisplayName())))
                E.SetMinRadius(E.BaseRadius.GetValue() + 2);

            if (E.Sense == PerceptionSense.Olfactory
                && ParentObject.GetBlueprint().InheritsFrom(ANIMAL_BLUEPRINT))
                E.SetMinRadius(E.BaseRadius.GetValue() + 2);

            if (E.Sense == PerceptionSense.Olfactory
                && ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.SetMinRadius(E.BaseRadius.GetValue() + Math.Min(heightenedSmell.Level, 5));

            if (E.Sense.EqualsAny(
                new PerceptionSense[]
                {
                    PerceptionSense.Visual,
                    PerceptionSense.Auditory,
                    PerceptionSense.Olfactory,
                })
                && E.Type.InheritsFrom(typeof(SimplePerception))
                && ParentObject.Body is Body body
                && body.LoopPart(SimplePerception.FACE_BODYPART, bp => !bp.IsDismembered) is List<BodyPart> facesList)
            {
                if (facesList.Count > 1)
                    E.SetMinRadius(E.Radius.GetValue() + 2);
                else
                if (facesList.Count < 1)
                    E.SetMaxRadius(0);
            }
            return base.HandleEvent(E);
        }
    }
}
