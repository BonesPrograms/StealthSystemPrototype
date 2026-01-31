using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Wish;
using XRL.UI;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

namespace XRL.World.Parts
{
    [HasWishCommand]
    [Serializable]
    public class UD_StealthHelper : IScribedPart
    {
        public static bool ConstantDebugOutput = false;

        public List<GameObject> Witnesses;

        public List<AwarenessLevel> WitnessesAwarenessLevels;

        public UD_StealthHelper()
        {
            Witnesses = null;
            WitnessesAwarenessLevels = null;
        }

        public static string PerceptionString(GameObject Perceiver, GameObject Hider)
            => Perceiver.MiniDebugName() + ": " +
            Perceiver
                ?.GetPart<UD_Witness>()
                ?.Perceptions
                ?.ToString(Short: true, Entity: Hider);

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

        public string WitnessListString(string Delimiter = "\n", Func<string, string> ProcItem = null)
            => Witnesses is List<GameObject> witnessess
                && witnessess.Count > 0
            ? witnessess?.Aggregate("", (a, n) => ProcWitness(a, n, Delimiter, ProcItem))
            : "Total {{K|Invisibility}}!";

        /*
        public List<GameObject> GetWitnesses()
        {
            GetWitnessesEvent.GetFor(ParentObject, ref Witnesses)
                ?.ForEach(delegate (GameObject witness)
                {
                    if (witness.TryGetPart(out UD_Witness witnessPart)
                        && witnessPart.BestPerception?.GetAwareness(ParentObject) is AwarenessLevel witnessAwareness)
                    {
                        WitnessesAwarenessLevels ??= new();
                        WitnessesAwarenessLevels.Add(witnessAwareness);
                    }
                });
            return Witnesses;
        }
        */

        #region Event Handling

        public override bool AllowStaticRegistration()
            => true;

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || (ConstantDebugOutput && ID == EndTurnEvent.ID)
            || ID == GetDebugInternalsEvent.ID
            ;
        public override bool HandleEvent(EndTurnEvent E)
        {
            if (ConstantDebugOutput
                && ParentObject.IsPlayer())
            {
                WitnessesAwarenessLevels.Clear();
                using Indent indent = new(1);
                Debug.Log("<UD_Steath debug toggle witnesses>", Indent: indent[0]);
                // GetWitnesses();
                Debug.Log(
                    Label: ParentObject.MiniDebugName() + " Witnesses",
                    Value: "\n" + WitnessListString("\n", s => " ".ThisManyTimes(4) + s),
                    Indent: indent[0]);
                Debug.Log("</UD_Steath debug toggle witnesses>", Indent: indent[0]);
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(Witnesses), WitnessListString().Strip());
            E.AddEntry(this, nameof(Witnesses) + " " + nameof(Witnesses.Count), Witnesses?.Count ?? 0);
            return base.HandleEvent(E);
        }
        public override bool Render(RenderEvent E)
        {
            if (ConstantDebugOutput
                && ParentObject.IsPlayer())
            {
                if (WitnessesAwarenessLevels.IsNullOrEmpty()
                    || WitnessesAwarenessLevels.All(l => l < AwarenessLevel.Awake))
                    E.ApplyColors("K", "w", int.MaxValue, int.MaxValue);
                else
                if (!WitnessesAwarenessLevels.IsNullOrEmpty()
                    && WitnessesAwarenessLevels.Any(l => l > AwarenessLevel.None))
                    E.ApplyColors("w", "W", int.MaxValue, int.MaxValue);
            }
            return base.Render(E);
        }

        #endregion
        #region Wishes

        [WishCommand(Command = "UD_Steath debug witnesses")]
        public static void DebugWitnesses_Wish()
        {
            if (The.Player is GameObject player
                && player.TryGetPart(out UD_StealthHelper stealthPart))
            {
                // stealthPart.GetWitnesses();
                string msg = player.MiniDebugName() + "'s Witnesses:\n" +
                    stealthPart.WitnessListString("\n", s => "{{K|" + "-".ThisManyTimes(4) + "}}" + s);
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
