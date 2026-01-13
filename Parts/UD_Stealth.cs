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

        public static string PerceptionString(GameObject Perceiver, GameObject Hider)
            => Perceiver.MiniDebugName() + ": " +
            Perceiver
                ?.GetPart<UD_Witness>()
                ?.Perceptions
                ?.ToString(Short: true, Entity: Hider, UseLastRoll: true, BestRollOnly: true);

        private string ProcWitness(
            string Accumulator,
            GameObject Next,
            string Delimiter = "\n",
            Func<string, string> ProcItem = null)
        {
            if (!Accumulator.IsNullOrEmpty())
                Accumulator += Delimiter;

            string nextString = PerceptionString(Next, ParentObject);
            if (ProcItem != null)
                nextString = ProcItem(nextString);

            return Accumulator + nextString;
        }

        public string WitnessListString(List<GameObject> Witnesses, string Delimiter = "\n", Func<string, string> ProcItem = null)
            => (Witnesses ?? this.Witnesses) is List<GameObject> witnessess
                && witnessess.Count > 0
            ? witnessess?.Aggregate("", (a, n) => ProcWitness(a, n, Delimiter, ProcItem))
            : "Total {{K|Invisibility}}!";

        public string WitnessListString(string Delimiter = "\n", Func<string, string> ProcItem = null)
            => WitnessListString(null, Delimiter, ProcItem);

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
                UnityEngine.Debug.Log("<UD_Steath debug toggle witnesses>");
                UnityEngine.Debug.Log(
                    ParentObject.MiniDebugName() + " Witnesses:\n" + 
                    WitnessListString(GetWitnesses(), "\n", s => " ".ThisManyTimes(4) + s));
                UnityEngine.Debug.Log("</UD_Steath debug toggle witnesses>");
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
                string msg = player.MiniDebugName() + " Witnesses:\n" +
                        stealthPart.WitnessListString(stealthPart.GetWitnesses(), "\n", s => "{{K|" + "-".ThisManyTimes(4) + "}}" + s);
                UnityEngine.Debug.Log("<UD_Steath debug witnesses>");
                UnityEngine.Debug.Log(msg.Strip().Replace("----", "    "));
                UnityEngine.Debug.Log("</UD_Steath debug witnesses>");
                Popup.Show(msg, LogMessage: false);
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
