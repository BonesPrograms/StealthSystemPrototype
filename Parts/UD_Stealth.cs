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
                ?.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? "\n" : null) + n.ToString());

        public string WitnessListString(string Delimiter = "\n")
            => Witnesses
                ?.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + PerceptionString(n))
            ?? "Invisible!";

        public List<GameObject> GetWitnesses()
            => Witnesses = GetWitnessesEvent.GetFor(ParentObject);

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
                GetWitnesses();
                UnityEngine.Debug.Log((ParentObject?.DebugName?.Strip() ?? "no one") + " Witnesses:\n" + WitnessListString());
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            GetWitnesses();
            E.AddEntry(this, nameof(Witnesses), WitnessListString());
            E.AddEntry(this, nameof(Witnesses) + " " + nameof(Witnesses.Count), Witnesses?.Count ?? 0);
            return base.HandleEvent(E);
        }

        #region Wishes
        [WishCommand(Command = "UD_Steath debug witnesses")]
        public static void DebugWitnesses_Wish()
        {
            if (The.Player is GameObject player
                && player.TryGetPart(out UD_Stealth stealthPart))
            {
                stealthPart.GetWitnesses();
                Popup.Show((player?.DebugName?.Strip() ?? "no one") + " Witnesses:\n" + stealthPart.WitnessListString());
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
