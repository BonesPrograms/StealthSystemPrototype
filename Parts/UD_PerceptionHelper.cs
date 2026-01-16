using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_PerceptionHelper 
        : IScribedPart
        , IPerceptionEventHandler
    {
        #region Static & Const

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

        #endregion
        #region Properties & Fields

        private PerceptionRack _Perceptions;
        public PerceptionRack Perceptions => _Perceptions ??= GetPerceptionsEvent.GetFor(ParentObject);

        #region Debugging

        private BasePerception _BestPerception;
        public BasePerception BestPerception => _BestPerception ??= Perceptions.GetHighestRatedPerceptionFor(The.Player);

        #endregion
        #endregion

        public UD_PerceptionHelper()
        {
            _Perceptions = null;
            _BestPerception = null;
        }

        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            Writer.WriteObject(_Perceptions);
            Writer.WriteObject(_BestPerception);
            base.Write(Basis, Writer);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            _Perceptions = Reader.ReadObject() as PerceptionRack;
            _BestPerception = Reader.ReadObject() as BasePerception;
            base.Read(Basis, Reader);
        }

        #endregion

        public void ClearPerceptions()
            => _Perceptions = null;

        public void ClearBestPerception()
            => _BestPerception = null;

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

            ClearPerceptions();
            return true;
        }
        private bool ClearPerceptions(Event E)
        {
            if (!IsClearPerceptionsStringyEvent(E.ID))
                return false;

            ClearPerceptions();
            return true;
        }

        public bool PerceptionsWantsEvent(int ID, int Cascade)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ID), ID),
                    Debug.Arg(nameof(Cascade), Cascade),
                });

            return Perceptions != null
            && Perceptions.WantDispatch(ID, Cascade);
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

            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(CallChain(nameof(Event), nameof(Event.ID)), E?.ID),
                });

            if (Perceptions != null
                && !Perceptions.FireEvent(E))
                return false;

            return base.FireEvent(E);
        }
        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || PerceptionsWantsEvent(ID, Cascade)
            || IsClearPerceptionsMinEvent(ID)
            || ID == GetPerceptionsEvent.ID
            || ID == GetPerceptionDieRollEvent.ID
            || ID == GetPerceptionRadiusEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;
        public override bool HandleEvent(MinEvent E)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E?.GetType()?.ToStringWithGenerics()),
                });

            return (Perceptions == null
                    || Perceptions.HandleEvent(E))
                && base.HandleEvent(E);
        }
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
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

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
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.GetType().ToStringWithGenerics()),
                    Debug.Arg(E.Name),
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

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
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.GetType().ToStringWithGenerics()),
                    Debug.Arg(E.Name),
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

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
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(
                Part: this,
                Name: nameof(Perceptions),
                Value: Perceptions?.ToStringLines(Short: true, Entity: The.Player, UseLastRoll: true) ?? "none??");
            return base.HandleEvent(E);
        }

        #endregion
    }
}
