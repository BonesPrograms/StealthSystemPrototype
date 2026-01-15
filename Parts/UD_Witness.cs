using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;

using StealthSystemPrototype.Events;

using XRL.World.AI.Pathfinding;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_Witness : IScribedPart, IModEventHandler<GetWitnessesEvent>
    {
        public static bool ConstantDebugOutput => UD_Stealth.ConstantDebugOutput;

        private Perceptions _Perceptions;

        public Perceptions Perceptions => _Perceptions ??= GetPerceptionsEvent.GetFor(ParentObject);

        private BasePerception _BestPerception;
        public BasePerception BestPerception => _BestPerception ??= Perceptions.GetHighestRatedPerceptionFor(The.Player);

        public bool PlayerPerceptable;

        public UD_Witness()
        {
            _Perceptions = null;
            _BestPerception = null;
            PlayerPerceptable = false;
        }

        public void ClearPerceptions()
            => _Perceptions = null;

        #region Event Handling

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == BeforeTakeActionEvent.ID
            || ID == GetWitnessesEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;
        public override bool HandleEvent(BeforeTakeActionEvent E)
        {
            if (ConstantDebugOutput && false)
                UnityEngine.Debug.Log(
                    (ParentObject?.DebugName?.Strip() ?? "no one") + " " + nameof(Perceptions) + ":\n" +
                    (Perceptions?.ToStringLines(Short: true) ?? "none??"));

            if (ConstantDebugOutput)
            {
                _BestPerception = null;
                PlayerPerceptable = The.Player is GameObject player
                    && player.TryGetPart(out UD_Stealth stealth)
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
                UnityEngine.Debug.Log(
                    (ParentObject?.DebugName ?? "null") + " " +
                    nameof(GetWitnessesEvent) + " -> " +
                    nameof(Perceptions) + " (" + (Perceptions?.Count ?? 0) + ")");

                if (Perceptions.GetAwareness(E.Hider, out BasePerception perception) > AwarenessLevel.None)
                {
                    UnityEngine.Debug.Log(" ".ThisManyTimes(4) + perception.ToString(Short: true));
                    E.AddWitness(perception);
                }
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
        public override bool Render(RenderEvent E)
        {
            if (ConstantDebugOutput
                && The.Player is GameObject player
                && BestPerception != null
                && BestPerception.GetAwareness(player) is AwarenessLevel playerAwareness)
            {
                if (playerAwareness > AwarenessLevel.None)
                    E.ApplyColors("Y", "y", 9999, 9999);
                else
                if (playerAwareness > AwarenessLevel.Awake)
                    E.ApplyColors("B", "b", 9999, 9999);
                else
                if (playerAwareness > AwarenessLevel.Suspect)
                    E.ApplyColors("R", "r", 9999, 9999);
            }
            return base.Render(E);
        }

        #endregion

        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            Writer.WriteObject(_Perceptions);
            Writer.WriteObject(_BestPerception);
            base.Write(Basis, Writer);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            _Perceptions = Reader.ReadObject() as Perceptions;
            _BestPerception = Reader.ReadObject() as BasePerception;
            base.Read(Basis, Reader);
        }

        #endregion
    }
}
