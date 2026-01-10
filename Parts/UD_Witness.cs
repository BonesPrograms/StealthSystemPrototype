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
            UnityEngine.Debug.Log(
                (ParentObject?.DebugName ?? "null") + " " + 
                nameof(GetWitnessesEvent) + " -> " + 
                nameof(Perceptions) + " (" + (Perceptions?.Count ?? 0) + ")");

            if (ParentObject != E.Hider
                && !ParentObject.InSamePartyAs(E.Hider)
                && E.Hider?.CurrentCell is Cell { InActiveZone: true } hiderCell
                && ParentObject.CurrentCell is Cell { InActiveZone: true } myCell
                && hiderCell.CosmeticDistanceto(myCell.Location) is int distance
                && Perceptions != null
                && Perceptions
                    ?.Aggregate(new List<int>(), delegate (List<int> Accumulator, BasePerception Next)
                    {
                        Accumulator.Add(Next.GetRadius());
                        return Accumulator;
                    }) is List<int> radii
                && radii.Any(r => r >= distance))
                E.AddWitness(this);
                
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(
                Part: this,
                Name: nameof(Perceptions),
                Value: Perceptions?.ToStringLines(Short: true, WithRolls: true) ?? "none??");
            return base.HandleEvent(E);
        }

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
