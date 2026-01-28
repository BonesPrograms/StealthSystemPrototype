using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Const;
using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_PerceptionHelper 
        : IScribedPart
        , IPerceptionEventHandler
        , IDetectionEventHandler
        , ISneakEventHandler
    {
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(XRL.World.Parts.UD_PerceptionHelper),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(WantEvent), false },
                    { nameof(ProcessPerceptionAlteringEvent), false },
                });
            Registry.RegisterHandleEventVariants(
                Type: typeof(XRL.World.Parts.UD_PerceptionHelper),
                MinEventTypeValues: new Dictionary<Type, bool>()
                {
                    { typeof(MinEvent), false },
                });
        }

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

        public static List<int> ValidatePerceptionsMinEvents => new()
        {
            RegenerateDefaultEquipmentEvent.ID,
            ImplantedEvent.ID,
            UnimplantedEvent.ID,
        };
        public static List<string> ValidatePerceptionsStringyEvents => new()
        {
            "MutationAdded",
        };

        #endregion
        #region Properties & Fields

        private PerceptionRack _Perceptions;
        public PerceptionRack Perceptions
        {
            get
            {
                if (_Perceptions == null)
                    GetPerceptionsEvent.GetFor(ParentObject, ref _Perceptions);
                return _Perceptions;
            }
        }

        public bool WantSync;

        #region Debugging

        private IPerception _BestPerception;
        public IPerception BestPerception => _BestPerception ??= Perceptions.GetHighestRatedPerceptionFor(The.Player);

        #endregion
        #endregion

        public UD_PerceptionHelper()
        {
            _Perceptions = null;
            WantSync = false;

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
            _BestPerception = Reader.ReadObject() as IPerception;
            base.Read(Basis, Reader);
        }

        #endregion

        public override IPart DeepCopy(GameObject Parent, Func<GameObject, GameObject> MapInv)
        {
            UD_PerceptionHelper perceptionHelper = base.DeepCopy(Parent, MapInv) as UD_PerceptionHelper;
            perceptionHelper._Perceptions = Perceptions.DeepCopy(Parent);
            return perceptionHelper;
        }

        public void SyncPerceptions()
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

            GetPerceptionsEvent.GetFor(ParentObject, ref _Perceptions);
            _Perceptions?.Validate();

            WantSync = false;
        }

        public void ClearBestPerception()
            => _BestPerception = null;

        private static bool IsClearPerceptionsMinEvent(int ID)
            => !ValidatePerceptionsMinEvents.IsNullOrEmpty()
            && ValidatePerceptionsMinEvents.Contains(ID);

        private static bool IsClearPerceptionsStringyEvent(string ID)
            => !ValidatePerceptionsStringyEvents.IsNullOrEmpty()
            && ValidatePerceptionsStringyEvents.Contains(ID);

        private bool ProcessPerceptionAlteringEvent(MinEvent E)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E?.TypeStringWithGenerics()),
                    Debug.Arg(ParentObject.DebugName ?? "null"),
                });

            if (!IsClearPerceptionsMinEvent(E.ID))
                return false;

            WantSync = true;
            return true;
        }
        private bool ProcessPerceptionAlteringEvent(Event E)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(CallChain(nameof(Event), nameof(Event.ID)), E?.ID),
                    Debug.Arg(ParentObject.DebugName ?? "null"),
                });

            if (!IsClearPerceptionsStringyEvent(E.ID))
                return false;

            WantSync = true;
            return true;
        }

        #region Event Handling

        public override bool AllowStaticRegistration()
            => true;

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            foreach (string stringyEvent in ValidatePerceptionsStringyEvents ?? new())
                Registrar.Register(stringyEvent);

            base.Register(Object, Registrar);
        }
        public override bool FireEvent(Event E)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(CallChain(nameof(Event), nameof(Event.ID)), E?.ID),
                });

            ProcessPerceptionAlteringEvent(E);

            if (Perceptions != null
                && !Perceptions.FireEvent(E))
                return false;

            return base.FireEvent(E);
        }
        public override bool WantEvent(int ID, int Cascade)
        {
            if (base.WantEvent(ID, Cascade))
                return true;

            if (IsClearPerceptionsMinEvent(ID))
                return true;

            if (ID.EqualsAny(
                args: new int[]
                {
                    GetPerceptionsEvent.ID,
                    AdjustTotalPerceptionLevelEvent.ID,
                    AdjustTotalPurviewEvent.ID,
                    TryConcealActionEvent.ID,
                    AfterDetectedEvent.ID,
                    GetDebugInternalsEvent.ID,
                }))
                return true;

            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(MinEvent.EventTypes[ID].ToStringWithGenerics()),
                    Debug.Arg(ParentObject.DebugName ?? "null"),
                });

            if (Perceptions?.HasWantEvent(ID, Cascade)
                ?? false)
                return true;

            return false;
        }
        public override bool HandleEvent(MinEvent E)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(MinEvent), E?.TypeStringWithGenerics()),
                });

            return base.HandleEvent(E)
                && (Perceptions?.DelegateHandleEvent(E)
                    ?? true);
        }
        public override bool HandleEvent(RegenerateDefaultEquipmentEvent E)
        {
            ProcessPerceptionAlteringEvent(E);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(ImplantedEvent E)
        {
            ProcessPerceptionAlteringEvent(E);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(UnimplantedEvent E)
        {
            ProcessPerceptionAlteringEvent(E);
            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetPerceptionsEvent E)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

            if (ParentObject.GetFirstBodyPart("Face") is BodyPart facePart)
            {
                E.RequireBodyPartPerception<VisualBodyPartPerception>(facePart, Level: 3, Purview: 5);
                E.RequireBodyPartPerception<AuditoryBodyPartPerception>(facePart, Level: 3, Purview: 4);
                E.RequireBodyPartPerception<OlfactoryBodyPartPerception>(facePart, Level: 3, Purview: 3);
            }


            if (ParentObject.TryGetPart(out Esper esper))
                E.RequirePerception(new EsperPsionicPerception(esper, 1, new EsperPurview(4)));

            /*
            E.RequirePerception(new SimpleVisualPerception(ParentObject));
            E.RequirePerception(new SimpleAuditoryPerception(ParentObject));
            E.RequirePerception(new SimpleOlfactoryPerception(ParentObject));

            if (ParentObject.TryGetPart(out HeightenedHearing heightenedHearing))
                E.RequireAuditoryIPartPerception(heightenedHearing);

            if (ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.RequireOlfactoryIPartPerception(heightenedSmell);

            if (ParentObject.TryGetPart(out NightVision nightVision))
                E.RequireVisualIPartPerception(nightVision);

            if (ParentObject.TryGetPart(out DarkVision darkVision))
                E.RequireVisualIPartPerception(darkVision);

            if (ParentObject.TryGetPart(out SensePsychic sensePsychic))
                E.RequirePsionicIPartPerception(sensePsychic);
            */

            Debug.Log(nameof(E.Perceptions), E.Perceptions?.Count ?? 0, Indent: indent[1]);

            return base.HandleEvent(E);
        }
        public bool HandleEvent(AdjustTotalPerceptionLevelEvent E)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.GetType().ToStringWithGenerics()),
                    Debug.Arg(E.Name),
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

            if (WantSync)
                SyncPerceptions();

            /*
            if (E.Alert.Name == nameof(Visual)
                && ParentObject.RequirePart<Mutations>() is var mutations
                && mutations.MutationList.Any(bm => VisionMutations.Contains(bm.GetDisplayName())))
                E.SetDieRollMin(40);

            if (E.Alert.Name == nameof(Olfactory)
                && ParentObject.GetBlueprint().InheritsFrom(ANIMAL_BLUEPRINT))
                E.SetDieRollMin(40);

            if (E.Alert.Name == nameof(Olfactory)
                && ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.AdjustDieRoll(2 * heightenedSmell.Level);
            */
            if (E.Type.Name.EqualsAny(
                new string[]
                {
                    nameof(SimpleVisualPerception),
                    nameof(SimpleAuditoryPerception),
                    nameof(SimpleOlfactoryPerception),
                })
                && ParentObject.Body is Body body
                && body.LoopPart(FACE_BODYPART, bp => !bp.IsDismembered) is List<BodyPart> facesList)
            {
                if (facesList.Count > 1)
                    E.AdjustByAmount(15);
                else
                if (facesList.Count < 1)
                    E.SetMaxValue(0);
            }
            return base.HandleEvent(E);
        }
        public bool HandleEvent(AdjustTotalPurviewEvent E)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(E.GetType().ToStringWithGenerics()),
                    Debug.Arg(E.Name),
                    Debug.Arg(ParentObject?.DebugName ?? "null"),
                });

            if (WantSync)
                SyncPerceptions();

            /*
            if (E.Alert.Name == nameof(Visual)
                && ParentObject.RequirePart<Mutations>() is var mutations
                && mutations.MutationList.Any(bm => VisionMutations.Contains(bm.GetDisplayName())))
                E.SetMinRadius(E.Purview.GetValue() + 2);

            if (E.Alert.Name == nameof(Olfactory)
                && ParentObject.GetBlueprint().InheritsFrom(ANIMAL_BLUEPRINT))
                E.SetMinRadius(E.Purview.GetValue() + 2);

            if (E.Alert.Name == nameof(Olfactory)
                && ParentObject.TryGetPart(out HeightenedSmell heightenedSmell))
                E.SetMinRadius(E.Purview.GetValue() + Math.Min(heightenedSmell.Level, 5));
            */
            return base.HandleEvent(E);
        }
        public virtual bool HandleEvent(TryConcealActionEvent E)
        {
            if (E.Hider != ParentObject
                && !E.Hider.InSamePartyAs(ParentObject)
                && !Perceptions.IsNullOrEmpty())
            {
                Perceptions.TryPerceive(E.ConcealedAction);
            }
            return base.HandleEvent(E);
        }
        public virtual bool HandleEvent(AfterDetectedEvent E)
        {
            if (ParentObject.BelongsToFaction(E.Perceiver.GetPrimaryFaction())
                && ParentObject != E.Perceiver
                && ParentObject != E.Hider)
                ParentObject?.Brain.CascadeOpinionDetection(Detection: E.Detection.DeepCopy(ParentObject));

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(
                Part: this,
                Name: nameof(Perceptions),
                Value: Perceptions?.ToStringLines(Short: true, Entity: The.Player) ?? "none??");
            return base.HandleEvent(E);
        }

        #endregion
    }
}
