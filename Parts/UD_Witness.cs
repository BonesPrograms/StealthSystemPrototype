using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World.AI;
using XRL.World.AI.Pathfinding;
using XRL.World.Effects;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_Witness : IScribedPart, IWitnessEventHandler
    {
        public static bool ConstantDebugOutput => UD_StealthHelper.ConstantDebugOutput;

        #region Properties & Fields

        private UD_PerceptionHelper PerceptionHelper => ParentObject?.GetPart<UD_PerceptionHelper>();

        public PerceptionRack Perceptions => ParentObject?.GetPerceptions();

        protected SneakerSet _ZoneSneakers;

        public SneakerSet ZoneSneakers
        {
            get => _ZoneSneakers ??= new(ParentObject);
            protected set => _ZoneSneakers = value;
        }

        public bool PlayerPerceptable
        {
            get
            {
                if (!The.Player.HasEffect<UD_Sneaking>())
                    return true;

                if (ParentObject.Brain.TryGetOpinions(The.Player, out OpinionList opinions)
                    && opinions.Any(o
                        => o is IOpinionDetection detectionOpinion
                        && detectionOpinion.Level >= AwarenessLevel.Aware))
                    return true;

                return false;
            }
        }

        #endregion

        public UD_Witness()
        {
            ZoneSneakers = null;
        }

        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here.
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here.
        }

        #endregion

        public override void Initialize()
        {
            ZoneSneakers = new(ParentObject);
            base.Initialize();
        }

        public void ClearSneakers()
            => _ZoneSneakers = null;

        #region Event Handling

        public override bool AllowStaticRegistration()
            => true;

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == BeforeTakeActionEvent.ID
            || ID == GetWitnessesEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;
        public override bool HandleEvent(BeforeTakeActionEvent E)
        {
            if (ConstantDebugOutput && false)
            {
                using Indent indent = new(1);
                Debug.Log((ParentObject?.DebugName?.Strip() ?? "no one") + " " + nameof(Perceptions) + ":", Indent: indent);
                Debug.Log(Perceptions?.ToString(Delimiter: indent[1] + "\n", Short: true, null) ?? indent[1] + "none??", Indent: indent);
            }
            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetWitnessesEvent E)
        {
            if (ParentObject != E.Hider
                && !ParentObject.InSamePartyAs(E.Hider))
            {
                E.AddWitness(this);

                using Indent indent = new(1);
                Debug.CheckYeh(Name, ParentObject?.DebugName ?? "NO_WITNESS", Indent: indent);
            }
                
            return base.HandleEvent(E);
        }
        public bool HandleEvent(IIsSneakingEvent E)
        {
            if (ParentObject != E.Hider
                && !ParentObject.InSamePartyAs(E.Hider))
            {
                if (ZoneSneakers != null
                    && E.Hider is ISneakSource hiderSneakSource
                    && !ZoneSneakers.Contains(hiderSneakSource))
                    ZoneSneakers.Add(hiderSneakSource);

                using Indent indent = new(1);
                Debug.CheckYeh(Name, ParentObject?.DebugName ?? "NO_WITNESS", Indent: indent);
            }
                
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            return base.HandleEvent(E);
        }
        public override bool Render(RenderEvent E)
        {
            if (ConstantDebugOutput
                && The.Player is GameObject player
                && !ParentObject.IsPlayer()
                && ParentObject != player
                && PlayerPerceptable
                )
            {
                /*
                if (PlayerAwareness > AwarenessLevel.Suspect)
                    E.ApplyColors("R", "r", int.MaxValue, int.MaxValue);
                else
                if (PlayerAwareness > AwarenessLevel.Awake)
                    E.ApplyColors("B", "b", int.MaxValue, int.MaxValue);
                else
                if (PlayerAwareness > AwarenessLevel.None)
                    E.ApplyColors("Y", "y", int.MaxValue, int.MaxValue);
                */
            }
            return base.Render(E);
        }

        #endregion
    }
}
