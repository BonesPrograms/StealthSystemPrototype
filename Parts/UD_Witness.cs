using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;

using StealthSystemPrototype.Events;

namespace XRL.World.Parts
{
    [Serializable]
    public class UD_Witness : IScribedPart, IModEventHandler<GetWitnessesEvent>
    {
        private Perceptions _Perceptions;

        public Perceptions Perceptions => _Perceptions ??= GetPerceptionsEvent.GetFor(ParentObject);

        public UD_Witness()
        {
            _Perceptions = null;
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
            if (UD_Stealth.ConstantDebugOutput && false)
                UnityEngine.Debug.Log(
                    (ParentObject?.DebugName?.Strip() ?? "no one") + " " + nameof(Perceptions) + ":\n" +
                    (Perceptions?.ToStringLines(Short: true) ?? "none??"));

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
                    UnityEngine.Debug.Log(" ".ThisManyTimes(4) + perception.ToString());
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

        #endregion

        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            Writer.WriteObject(_Perceptions);
            base.Write(Basis, Writer);
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            _Perceptions = Reader.ReadObject() as Perceptions;
            base.Read(Basis, Reader);
        }

        #endregion
    }
}
