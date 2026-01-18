using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Capabilities.Stealth.Sneak;

using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects
{
    public class UD_Sneaking : IScribedEffect, ITierInitialized
    {
        public const string DISPLAY_NAME = "{{K|light footed}}";
        public const string VERBING = "sneaking";

        public SneakPerformance SneakPerformance => Object?.GetPart<UD_Sneak>()?.SneakPerformance;

        public bool PenaltyApplied;

        protected int AppliedMoveSpeedPenalty;

        public UD_Sneaking()
        {
            DisplayName = DISPLAY_NAME;
            Duration = DURATION_INDEFINITE;
            AppliedMoveSpeedPenalty = 0;
        }

        public void Initialize(int Tier)
        {
            DisplayName = DISPLAY_NAME;
        }

        public override int GetEffectType()
            => TYPE_MINOR
            | TYPE_REMOVABLE
            | TYPE_VOLUNTARY;

        public override bool Apply(GameObject Object)
        {
            if (Object.HasEffect<UD_Sneaking>()
                && !Object.CanChangeMovementMode(VERBING)
                && !Object.FireEvent(Event.New(nameof(Apply) + nameof(UD_Sneaking), "Effect", this)))
                return false;

            Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_movementBuff");
            StatShifter.DefaultDisplayName = DisplayName;
            Object.MovementModeChanged(VERBING);
            DidX("begin", VERBING, "!");
            RecalcMovespeedBonus();
            return true;
        }

        public void RecalcMovespeedBonus()
        {
            if (Duration > 0)
            {
                StatShifter.RemoveStatShift(Object, "MoveSpeed");
                int num = 100 - Object.Stat("MoveSpeed") + 100;
                AppliedMoveSpeedPenalty = (int)((float)num * (GetMovespeedMultiplier() - 1f));
                StatShifter.SetStatShift("MoveSpeed", -MovespeedBonus);
            }
        }

        public static float GetMovespeedMultiplier(GameObject Object, SneakPerformance SneakPerformance)
        {
            float penalty;
            if (SpringingEffective && Object.HasEffect<Springing>())
            {
                stats?.AddPercentageBonusModifier("Multiplier", 100, "springing effect");
                penalty = 3f;
            }
            else
            {
                penalty = 2f;
            }
            Wings part = Object.GetPart<Wings>();
            if (part != null)
            {
                float num2 = part.SprintingMoveSpeedBonus(part.Level);
                stats?.AddPercentageBonusModifier("Multiplier", (int)(num2 * 100f), part.GetDisplayName() + " " + part.GetMutationTerm());
                num *= 1f + part.SprintingMoveSpeedBonus(part.Level);
            }
            stats?.CollectBonusModifiers("Multiplier", 100, "Move speed multiplier");
            stats?.Set("Multipler", (int)((num - 1f) * 100f), num != 2f, (num > 2f) ? 1 : (-1));
            return num;
        }
    }
}
