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
        , IPerceptionEventHandler
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

        public static List<int> ClearPerceptionsMinEvents => new()
        {
            RegenerateDefaultEquipmentEvent.ID,
            ImplantedEvent.ID,
            UnimplantedEvent.ID,
        };
        public static List<string> ClearPerceptionsStringyEvents => new()
        {
            "MutationAdded",
        };

        public UD_Witness WitnessPart => ParentObject?.GetPart<UD_Witness>();

        public UD_PerceptionHelper()
        {
        }

        #region Event Handling

        private static bool IsClearPerceptionsMinEvent(int ID)
            => !ClearPerceptionsMinEvents.IsNullOrEmpty()
            && ClearPerceptionsMinEvents.Contains(ID);

        private static bool IsClearPerceptionsStringyEvent(string ID)
            => !ClearPerceptionsStringyEvents.IsNullOrEmpty()
            && ClearPerceptionsStringyEvents.Contains(ID);

        private bool ProcessMutationAddedEvent(MinEvent E)
        {
            if (!IsClearPerceptionsMinEvent(E.ID))
                return false;

            WitnessPart?.ClearPerceptions();
            return true;
        }
        private bool ClearPerceptions(Event E)
        {
            if (!IsClearPerceptionsStringyEvent(E.ID))
                return false;

            WitnessPart?.ClearPerceptions();
            return true;
        }

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            foreach (string stringyEvent in ClearPerceptionsStringyEvents ?? new())
                Registrar.Register(stringyEvent);

            base.Register(Object, Registrar);
        }
        public override bool FireEvent(Event E)
        {
            ClearPerceptions(E);
            return base.FireEvent(E);
        }
        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || IsClearPerceptionsMinEvent(ID)
            || ID == GetPerceptionsEvent.ID
            || ID == GetPerceptionDieRollEvent.ID
            || ID == GetPerceptionRadiusEvent.ID
            ;
        public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
        {
            ProcessMutationAddedEvent(E);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ImplantedEvent E)
        {
            ProcessMutationAddedEvent(E);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(UnimplantedEvent E)
        {
            ProcessMutationAddedEvent(E);
            return base.HandleEvent(E);
        }
        public virtual bool HandleEvent(GetPerceptionsEvent E)
        {
            E.AddPerception(new Visual(ParentObject));
            E.AddPerception(new Auditory(ParentObject));
            E.AddPerception(new Olfactory(ParentObject));

            if (ParentObject.TryGetPart(out Esper esper))
                E.AddPerception(new EsperPsionic(esper));

            if (ParentObject.TryGetPart(out HeightenedHearing heightenedHearing))
                E.AddAuditoryIPartPerception(heightenedHearing);

            if (ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.AddOlfactoryIPartPerception(heightenedSmell);

            if (ParentObject.TryGetPart(out NightVision nightVision))
                E.AddVisualIPartPerception(nightVision);

            if (ParentObject.TryGetPart(out DarkVision darkVision))
                E.AddVisualIPartPerception(darkVision);

            if (ParentObject.TryGetPart(out SensePsychic sensePsychic))
                E.AddPsionicIPartPerception(sensePsychic);

            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetPerceptionsEvent) + " -> " + 
                nameof(E.Perceptions) + " (" + (E.Perceptions?.Count ?? 0) + ")");

            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetPerceptionDieRollEvent E)
        {
            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetPerceptionDieRollEvent) + " -> " + 
                (E.Name ?? "no type?"));

            if (E.Sense == PerceptionSense.Visual
                && ParentObject.RequirePart<Mutations>() is var mutations
                && mutations.MutationList.Any(bm => VisionMutations.Contains(bm.GetDisplayName())))
                E.SetDieRollMin(40);

            if (E.Sense == PerceptionSense.Olfactory
                && ParentObject.GetBlueprint().InheritsFrom(ANIMAL_BLUEPRINT))
                E.SetDieRollMin(40);

            if (E.Sense == PerceptionSense.Olfactory
                && ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.AdjustDieRoll(2 * heightenedSmell.Level);

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
                    E.AdjustDieRoll(15);
                else
                if (facesList.Count < 1)
                    E.SetDieRollMax(0);
            }
            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetPerceptionRadiusEvent E)
        {
            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetPerceptionRadiusEvent) + " -> " + 
                (E.Name ?? "no type?"));

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

        #endregion
    }
}
