using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Wish;
using XRL.UI;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events;

namespace XRL.World.Parts
{
    [HasWishCommand]
    [Serializable]
    public class UD_Stealth : IScribedPart
    {
        public static bool ConstantDebugOutput = false;

        public List<GameObject> Witnesses;

        public UD_Stealth()
        {
            Witnesses = null;
        }

        public static string PerceptionString(GameObject Object)
            => Object
                ?.GetPart<UD_Witness>()
                ?.Perceptions
                ?.ToString(Short: true, WithRolls: true);

        public string WitnessListString(List<GameObject> Witnesses, string Delimiter = "\n")
            => (Witnesses ?? this.Witnesses)
                ?.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + PerceptionString(n))
            ?? "Total {{K|Invisibility}}!";

        public string WitnessListString(string Delimiter = "\n")
            => WitnessListString(null, Delimiter);

        public List<GameObject> GetWitnesses()
            => Witnesses = GetWitnessesEvent.GetFor(ParentObject);

        #region Event Handling

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || (ConstantDebugOutput && ID == BeginTakeActionEvent.ID)
            || ID == GetDebugInternalsEvent.ID
            ;
        public override bool HandleEvent(BeginTakeActionEvent E)
        {
            if (E.Object == ParentObject
                && ParentObject.IsPlayer())
            {
                UnityEngine.Debug.Log(
                    (ParentObject?.DebugName?.Strip() ?? "no one") + " Witnesses:\n" + 
                    WitnessListString(GetWitnesses(), "\n    "));
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(Witnesses), WitnessListString(GetWitnesses()));
            E.AddEntry(this, nameof(Witnesses) + " " + nameof(Witnesses.Count), Witnesses?.Count ?? 0);
            return base.HandleEvent(E);
        }

        #endregion

        #region Wishes

        [WishCommand(Command = "UD_Steath debug witnesses")]
        public static void DebugWitnesses_Wish()
        {
            if (The.Player is GameObject player
                && player.TryGetPart(out UD_Stealth stealthPart))
            {
                ;
                Popup.Show(
                    Message: (player?.DebugName?.Strip() ?? "no one") + " Witnesses:\n" + 
                        stealthPart.WitnessListString(stealthPart.GetWitnesses(), "\n{{K|----}}"),
                    LogMessage: false);
            }
        }
        [WishCommand(Command = "UD_Steath debug toggle witnesses")]
        public static void DebugToggleWitnesses_Wish()
        {
            ConstantDebugOutput = !ConstantDebugOutput;
        }

        #endregion
    }
}
