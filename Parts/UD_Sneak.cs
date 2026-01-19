using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Wish;
using XRL.UI;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Capabilities.Stealth.Sneak;
using XRL.World.Effects;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts
{
    [HasWishCommand]
    [Serializable]
    public class UD_Sneak : IScribedPart, ISneakEventHandler
    {
        public static string SUPPORT_TYPE => nameof(UD_Sneak);
        public static string COMMAND_SNEAK => "CommandToggleSneaking";

        private SneakPerformance _SneakPerformance;
        public SneakPerformance SneakPerformance
        {
            get => _SneakPerformance ??= GetSneakPerformanceEvent.GetFor(ParentObject);
            protected set => _SneakPerformance = value;
        }

        public Guid ActivatedAbilityID;

        public UD_Sneak()
        {
            SneakPerformance = null;
            ActivatedAbilityID = Guid.Empty;
        }

        public override void Remove()
        {
            RemoveMyActivatedAbility(ref ActivatedAbilityID);
            base.Remove();
        }

        public void SyncAbility(bool Silent = false)
        {
            if (SneakPerformance.WantsSync)
            {
                SneakPerformance = null;
                if (ParentObject.TryGetEffect(out UD_Sneaking sneaking))
                    sneaking.ClearDetailsEntries();
            }

            bool removed = false;
            if (ParentObject.GetActivatedAbilityByCommand(COMMAND_SNEAK) is ActivatedAbilityEntry abilityEntry
                && abilityEntry.ID != ActivatedAbilityID)
            {
                removed = RemoveMyActivatedAbility(ref ActivatedAbilityID) || removed;
                removed = RemoveMyActivatedAbility(ref abilityEntry.ID) || removed;

                ParentObject.RemoveAllEffects<UD_Sneaking>();
            }

            if (ActivatedAbilityID == Guid.Empty)
            {
                ActivatedAbilityID = AddMyActivatedAbility(
                    Name: "Sneak",
                    Command: COMMAND_SNEAK,
                    Class: "Maneuvers",
                    Description: null, // write one into the xml files. Remove this comment when done.
                    Icon: "\u001a",
                    Toggleable: true,
                    ActiveToggle: true,
                    Silent: Silent || removed);
            }
        }
        public static void SyncAbility(GameObject Who, bool Silent = false)
            => Who.GetPart<UD_Sneak>()?.SyncAbility(Silent);

        public bool IsSneaking()
            => ParentObject.HasEffect<UD_Sneaking>();

        public bool StartSneaking()
            => !IsSneaking()
            && ParentObject.CheckFrozen()
            && ParentObject.CanChangeMovementMode("sneak", ShowMessage: true)
            && ParentObject.CheckNotOnWorldMap("sneak", ShowMessage: true)
            && ParentObject.ApplyEffect(new UD_Sneaking())
            && ToggleMyActivatedAbility(ActivatedAbilityID, SetState: true);
            // CooldownMyActivatedAbility(ActivatedAbilityID, 100, null, "Intelligence");

        public bool StopSneaking()
            => IsSneaking()
            && ParentObject.RemoveAllEffects<UD_Sneaking>() > 0
            && ToggleMyActivatedAbility(ActivatedAbilityID, SetState: false);

        public bool ToggleSneaking()
            => !IsSneaking()
            ? StartSneaking()
            : StopSneaking();

        #region Event Handling

        public override bool AllowStaticRegistration()
            => true;

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            || ID == NeedPartSupportEvent.ID
            || ID == CommandEvent.ID
            || ID == GetSneakPerformanceEvent.ID
            ;
        public override bool HandleEvent(NeedPartSupportEvent E)
        {
            if (E.Type == SUPPORT_TYPE
                && !PartSupportEvent.Check(E, this))
                ParentObject.RemovePart(this);

            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CommandEvent E)
        {
            if (E.Command == COMMAND_SNEAK
                && !ToggleSneaking())
                return false;

            return base.HandleEvent(E);
        }
        public virtual bool HandleEvent(GetSneakPerformanceEvent E)
        {
            if (E.Hider == ParentObject
                && !E.Hider.HasSkill(nameof(UD_Stealth_LightFooted)))
                E.AdjustMoveSpeedMultiplier(this, -10);
            return base.HandleEvent(E);
        }

        #endregion
    }
}
