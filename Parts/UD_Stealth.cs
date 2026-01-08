using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Wish;
using XRL.UI;

using StealthSystemPrototype;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Events.Perception;
using StealthSystemPrototype.Events.Witness;

using static StealthSystemPrototype.Capabilities.Stealth.Perception;

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

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || (ConstantDebugOutput && ID == BeginTakeActionEvent.ID)
            ;

        public override bool HandleEvent(BeginTakeActionEvent E)
        {
            if (E.Object == The.Player)
            {
                if (GetWitnessesEvent.GetFor(The.Player) is List<GameObject> witnessList)
                    UnityEngine.Debug.Log("Witnesses:\n" + witnessList.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? "\n" : null) + (n?.DebugName ?? "null?")));
                else
                    UnityEngine.Debug.Log("no witnesses");

            }
            return base.HandleEvent(E);
        }

        #region Wishes
        [WishCommand(Command = "Bones_Steath debug witnesses")]
        public static void DebugWitnesses_Wish()
        {
            if (The.Player is GameObject player)
            {
                if (GetWitnessesEvent.GetFor(player) is List<GameObject> witnessList)
                {
                    Popup.Show("Witnesses:\n" + witnessList.Aggregate("", (a, n) => a + (!a.IsNullOrEmpty() ? "\n" : null) + (n?.DebugName ?? "null?")));
                }
                else
                    Popup.Show("no witnesses");
            }
        }
        [WishCommand(Command = "Bones_Steath debug toggle witnesses")]
        public static void DebugToggleWitnesses_Wish()
        {
            ConstantDebugOutput = !ConstantDebugOutput;
        }
        #endregion
    }
}
