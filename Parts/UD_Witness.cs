using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using XRL.World.AI.Pathfinding;
using StealthSystemPrototype.Senses;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_Witness : IScribedPart, IWitnessEventHandler
    {
        public static bool ConstantDebugOutput => UD_StealthHelper.ConstantDebugOutput;

        #region Properties & Fields

        private UD_PerceptionHelper PerceptionHelper => ParentObject?.GetPart<UD_PerceptionHelper>();

        public PerceptionRack Perceptions => ParentObject?.GetPerceptions();

        #region Debugging

        public IPerception BestPerception => PerceptionHelper?.BestPerception;

        public bool PlayerPerceptable;

        #endregion
        #endregion

        public UD_Witness()
        {
            PlayerPerceptable = false;
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

        public void ClearPerceptions()
            => PerceptionHelper.SyncPerceptions();

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

            if (ConstantDebugOutput
                && !ParentObject.IsPlayer())
            {
                PerceptionHelper?.ClearBestPerception();

                PlayerPerceptable = The.Player is GameObject player
                    && player.TryGetPart(out UD_StealthHelper stealth)
                    && !stealth.Witnesses.IsNullOrEmpty()
                    && stealth.Witnesses.Contains(ParentObject)
                    && BestPerception.CheckInRadius(player, out int _, out FindPath _);
            }
            else
            {
                PlayerPerceptable = false;
            }
            return base.HandleEvent(E);
        }
        public bool HandleEvent(GetWitnessesEvent E)
        {
            if (ParentObject != E.Hider
                && !ParentObject.InSamePartyAs(E.Hider))
            {
                using Indent indent = new(1);
                Debug.LogCaller(indent,
                    ArgPairs: new Debug.ArgPair[]
                    {
                        Debug.Arg(E.GetType().ToStringWithGenerics()),
                        Debug.Arg(ParentObject?.DebugName ?? "null"),
                        Debug.Arg(nameof(Perceptions), Perceptions?.Count ?? 0),
                    });

                /*
                if (Perceptions.Sense(E.Hider, out IPerception perception) > AwarenessLevel.None)
                {
                    Debug.Log(perception.ToString(Short: true), Indent: indent[1]);
                    E.AddWitness(perception);
                }
                */
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
                && BestPerception != null
                && BestPerception.GetAwareness(player) is AwarenessLevel playerAwareness)
            {
                if (playerAwareness > AwarenessLevel.Suspect)
                    E.ApplyColors("R", "r", int.MaxValue, int.MaxValue);
                else
                if (playerAwareness > AwarenessLevel.Awake)
                    E.ApplyColors("B", "b", int.MaxValue, int.MaxValue);
                else
                if (playerAwareness > AwarenessLevel.None)
                    E.ApplyColors("Y", "y", int.MaxValue, int.MaxValue);
            }
            return base.Render(E);
        }

        #endregion
    }
}
