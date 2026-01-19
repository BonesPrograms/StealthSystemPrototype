using System;
using System.Collections.Generic;
using System.Text;

using XRL.Collections;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

using SerializeField = UnityEngine.SerializeField;

using StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Events;
using System.Linq;

namespace XRL.World.Effects
{
    public class UD_Sneaking : IScribedEffect, ITierInitialized, ISneakEventHandler
    {
        public const string DISPLAY_NAME = "{{K|light footed}}";
        public const string VERBING = "sneaking";

        public SneakPerformance SneakPerformance => Object?.GetPart<UD_Sneak>()?.SneakPerformance;

        [SerializeField]
        private StringMap<string> _DetailsEntries;
        public StringMap<string> DetailsEntries => _DetailsEntries ??= GetSneakDetailsEvent.GetFor(Object);

        [SerializeField]
        private bool IsMoveSpeedMultiplierApplied;

        [SerializeField]
        private int AppliedMoveSpeedMultiplierAmount;

        public bool IsBeingPerceived;

        public UD_Sneaking()
        {
            DisplayName = DISPLAY_NAME;
            Duration = DURATION_INDEFINITE;
            AppliedMoveSpeedMultiplierAmount = 0;
            IsBeingPerceived = false;
            _DetailsEntries = null;
        }

        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        #endregion

        public override bool SameAs(Effect FX)
            => false;

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

        public override void Remove(GameObject Object)
        {
            DidX("stop", VERBING);
            StatShifter.RemoveStatShifts(Object);
            base.Remove(Object);
        }

        public override string GetDetails()
        {
            StringBuilder SB = Event.NewStringBuilder();

            if (!DetailsEntries.IsNullOrEmpty())
                foreach ((string _, string entry) in DetailsEntries)
                SB.Compound(entry, '\n');

            return SB.ToString();
        }

        public void ClearDetailsEntries()
            => _DetailsEntries = null;

        public static int GetMoveSpeedShiftAmount(GameObject Object, float Multiplier)
            => (int)((100 - Object.Stat("MoveSpeed") + 100) * (Multiplier - 1f));

        public void RecalcMovespeedBonus()
        {
            if (Duration > 0)
            {
                ClearDetailsEntries();
                StatShifter.RemoveStatShift(Object, "MoveSpeed");
                AppliedMoveSpeedMultiplierAmount = GetMoveSpeedShiftAmount(Object, GetMovespeedMultiplier());
                if (!(IsMoveSpeedMultiplierApplied = StatShifter.SetStatShift("MoveSpeed", -AppliedMoveSpeedMultiplierAmount)))
                    AppliedMoveSpeedMultiplierAmount = 0;
            }
        }

        public static float GetMovespeedMultiplier(GameObject Object, SneakPerformance SneakPerformance, Templates.StatCollector stats = null)
        {
            float multiplier = 1f;
            if ((SneakPerformance ??= Object?.GetPart<UD_Sneak>()?.SneakPerformance) != null)
            {
                foreach ((int multi, string source) in SneakPerformance.GetCollectedStats(SneakPerformance.MOVESPEED_MULTI))
                    stats?.AddPercentageBonusModifier(SneakPerformance.MOVESPEED_MULTI, multi, source);

                multiplier += SneakPerformance.MoveSpeedMultiplier;
                stats?.Set(SneakPerformance.MOVESPEED_MULTI, (int)((multiplier - 1f) * 100f), multiplier != 1f, multiplier.CompareTo(1f));
            }
            return multiplier;
        }

        public float GetMovespeedMultiplier()
            => GetMovespeedMultiplier(Object, SneakPerformance);

        #region Event Handling

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("BodyPositionChanged");
            Registrar.Register("MovementModeChanged");
            Registrar.Register(GetSneakDetailsEvent.ID, EventOrder.EXTREMELY_EARLY);
            base.Register(Object, Registrar);
        }
        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            // || ID == GetSneakDetailsEvent.ID
            || ID == EndTurnEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;
        public virtual bool HandleEvent(GetSneakDetailsEvent E)
        {
            if (IsMoveSpeedMultiplierApplied)
            {
                StringBuilder SB = Event.NewStringBuilder();
                SB.Compound("Moves at " + GetMovespeedMultiplier() + "X the normal speed. (" + AppliedMoveSpeedMultiplierAmount.Signed() + " move speed)", '\n');
                if (SneakPerformance != null)
                {
                    IList<(int multi, string source)> entries = (IList<(int, string)>)SneakPerformance
                        ?.GetCollectedStats(SneakPerformance.MOVESPEED_MULTI)
                        ?.Select(e => ((100 - e.Value) / 100f, e.Source));

                    if (!entries.IsNullOrEmpty())
                    {
                        int count = entries.Count;
                        SB.Compound("Multiplier sources:", '\n');
                        for (int i = 0; i < count; i++)
                            SB.Compound(
                                Text: entries[i].source + ": " + entries[i].multi + "X (" + GetMoveSpeedShiftAmount(Object, entries[i].multi).Signed(), 
                                With: (i < count - 1 ? "\n" : null));
                    }
                }
                E.Add(this, SB.ToString());
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(EndTurnEvent E)
        {
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(IsMoveSpeedMultiplierApplied), IsMoveSpeedMultiplierApplied);
            E.AddEntry(this, nameof(AppliedMoveSpeedMultiplierAmount), AppliedMoveSpeedMultiplierAmount);
            E.AddEntry(this, SneakPerformance.PerformanceEntriesDebugString(out string performanceEntriesContents), performanceEntriesContents);
            E.AddEntry(this, SneakPerformance.CollectedStatsEntriesDebugString(out string collectedStatsEntriesContents), collectedStatsEntriesContents);
            return base.HandleEvent(E);
        }
        public override bool Render(RenderEvent E)
        {
            if (Duration > 0)
            {
                if (!IsBeingPerceived)
                    E.ApplyColors("k", "K", int.MaxValue, int.MaxValue);
                else
                    E.ApplyColors("K", "W", int.MaxValue, int.MaxValue);

                E.RenderEffectIndicator("_", "Tiles2/status_sprinting.bmp", "&w", "K", 35);
            }
            return base.Render(E);
        }
        public override bool FireEvent(Event E)
        {
            if (E.ID == "MovementModeChanged"
                || E.ID == "BodyPositionChanged")
                if (E.GetStringParameter("To") is string changedTo)
                {
                    if (changedTo == "Frozen")
                        Object.RemoveEffect(this);
                    else
                    if (changedTo == "Jumping"
                        && !Object.HasPart<Tactics_Hurdle>())
                        Object.RemoveEffect(this);
                    else
                        RecalcMovespeedBonus();
                }

            return base.FireEvent(E);
        }

        #endregion
    }
}
