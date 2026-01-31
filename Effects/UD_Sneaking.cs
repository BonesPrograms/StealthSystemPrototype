using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using XRL.Collections;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;
using XRL.World.Parts.Skill;

using SerializeField = UnityEngine.SerializeField;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth;

using static StealthSystemPrototype.Capabilities.Stealth.SneakPerformance;

namespace XRL.World.Effects
{
    public class UD_Sneaking : IScribedEffect, ITierInitialized, ISneakEventHandler
    {
        public const string DISPLAY_NAME = "{{K|light footed}}";
        public const string VERBING = "sneaking";

        public const string MS_NAME = "MoveSpeed";
        public const string QN_NAME = "Speed";

        public static Dictionary<string, ConcealedCommandAction> CommandEventsToConceal => new()
        {
            { Survival_Camp.COMMAND_NAME, new ConcealedCommandAction(false, "making camp") {
                new Visual(8),
                new Auditory(3),
                new Olfactory(5),
            }.Initialize() as ConcealedCommandAction },
        };

        public GameObject Source;

        public SneakPerformance SneakPerformance => Object?.GetPart<UD_Sneak>()?.SneakPerformance;

        [SerializeField]
        private StringMap<string> _DetailsEntries;
        public StringMap<string> DetailsEntries => _DetailsEntries ??= GetSneakDetailsEvent.GetFor(Object);

        [SerializeField]
        private bool IsMoveSpeedMultiplierApplied;

        [SerializeField]
        private int AppliedMoveSpeedMultiplierAmount;

        [SerializeField]
        private bool IsQuicknessMultiplierApplied;

        [SerializeField]
        private int AppliedQuicknessMultiplierAmount;

        public bool IsBeingPerceived;

        public UD_Sneaking()
        {
            Source = null;
            DisplayName = DISPLAY_NAME;
            Duration = DURATION_INDEFINITE;
            IsMoveSpeedMultiplierApplied = false;
            AppliedMoveSpeedMultiplierAmount = 0;
            IsQuicknessMultiplierApplied = false;
            AppliedQuicknessMultiplierAmount = 0;
            IsBeingPerceived = false;
            _DetailsEntries = null;
        }

        public UD_Sneaking(GameObject Source)
            : this()
        {
            this.Source = Source;
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
            UD_StealthHelper stealthHelperPart = Object.RequirePart<UD_StealthHelper>();
            string abortedByEventMessage = null;
            if (!BeforeSneakEvent.Check(Object, SneakPerformance, ref stealthHelperPart.Witnesses, ref abortedByEventMessage))
            {
                if (!abortedByEventMessage.IsNullOrEmpty())
                    return Object.ShowFailure(abortedByEventMessage);
                return false;
            }

            if (Object.HasEffect<UD_Sneaking>()
                && !Object.CanChangeMovementMode(VERBING)
                && !Object.FireEvent(Event.New(nameof(Apply) + nameof(UD_Sneaking), "Effect", this)))
                return false;

            Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_movementBuff");
            StatShifter.DefaultDisplayName = DisplayName;
            Object.MovementModeChanged(VERBING);
            DidX("begin", VERBING, "!");
            RecalcStatMultipliers();
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
            => (int)((100 - Object.Stat(MS_NAME) + 100) * (Multiplier - 1f));

        public static int GetQuicknessShiftAmount(GameObject Object, float Multiplier)
            => (int)(Object.Stat(QN_NAME) * (Multiplier - 1f));

        public void RecalcStatMultipliers()
        {
            if (Duration > 0)
            {
                ClearDetailsEntries();
                StatShifter.RemoveStatShift(Object, MS_NAME);
                AppliedMoveSpeedMultiplierAmount = GetMoveSpeedShiftAmount(Object, GetMovespeedMultiplier());
                if (!(IsMoveSpeedMultiplierApplied = StatShifter.SetStatShift(MS_NAME, -AppliedMoveSpeedMultiplierAmount)))
                    AppliedMoveSpeedMultiplierAmount = 0;

                StatShifter.RemoveStatShift(Object, QN_NAME);
                AppliedQuicknessMultiplierAmount = GetQuicknessShiftAmount(Object, GetQuicknessMultiplier());
                if (!(IsQuicknessMultiplierApplied = StatShifter.SetStatShift(QN_NAME, AppliedQuicknessMultiplierAmount)))
                    AppliedQuicknessMultiplierAmount = 0;
            }
        }

        public static float GetMovespeedMultiplier(GameObject Object, SneakPerformance SneakPerformance, Templates.StatCollector stats = null)
        {
            float multiplier = 1f;
            if ((SneakPerformance ??= Object?.GetPart<UD_Sneak>()?.SneakPerformance) != null)
            {
                foreach ((int multi, string source) in SneakPerformance.GetCollectedStats(MS_MULTI))
                    stats?.AddPercentageBonusModifier(MS_MULTI, multi, source);

                multiplier += SneakPerformance.MoveSpeedMultiplier;
                stats?.Set(MS_MULTI, (int)((multiplier - 1f) * 100f), multiplier != 1f, multiplier.CompareTo(1f));
            }
            return multiplier;
        }
        public float GetMovespeedMultiplier()
            => GetMovespeedMultiplier(Object, SneakPerformance);

        public static float GetQuicknessMultiplier(GameObject Object, SneakPerformance SneakPerformance, Templates.StatCollector stats = null)
        {
            float multiplier = 1f;
            if ((SneakPerformance ??= Object?.GetPart<UD_Sneak>()?.SneakPerformance) != null)
            {
                foreach ((int multi, string source) in SneakPerformance.GetCollectedStats(QN_MULTI))
                    stats?.AddPercentageBonusModifier(QN_MULTI, multi, source);

                multiplier += SneakPerformance.QuicknessMultiplier;
                stats?.Set(QN_MULTI, (int)((multiplier - 1f) * 100f), multiplier != 1f, multiplier.CompareTo(1f));
            }
            return multiplier;
        }
        public float GetQuicknessMultiplier()
            => GetQuicknessMultiplier(Object, SneakPerformance);

        #region Event Handling

        public override void Register(GameObject Object, IEventRegistrar Registrar)
        {
            Registrar.Register("BodyPositionChanged");
            Registrar.Register("MovementModeChanged");
            Registrar.Register(CommandEvent.ID, EventOrder.EXTREMELY_LATE);
            Registrar.Register(GetSneakDetailsEvent.ID, EventOrder.EXTREMELY_EARLY);
            base.Register(Object, Registrar);
        }
        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            // || ID == GetSneakDetailsEvent.ID
            || ID == EndTurnEvent.ID
            || ID == EnteredCellEvent.ID
            || ID == GetDebugInternalsEvent.ID
            ;
        public virtual bool HandleEvent(GetSneakDetailsEvent E)
        {
            if (SneakPerformance != null)
            {
                if (IsMoveSpeedMultiplierApplied
                || IsQuicknessMultiplierApplied)
                {
                    StringBuilder SB = Event.NewStringBuilder();
                    if (IsMoveSpeedMultiplierApplied)
                    {
                        if (!SB.IsNullOrEmpty())
                            SB.AppendLine().AppendLine();

                        float totalMSMulti = GetMovespeedMultiplier();
                        bool mSTotalGood = totalMSMulti >= 1f;
                        SB.Append("Moves at ").AppendColored(mSTotalGood ? "g" : "r", totalMSMulti.ToString()).Append("X the normal speed. ")
                            .Append("(").AppendColored(mSTotalGood ? "g" : "r", AppliedMoveSpeedMultiplierAmount.Signed()).Append(" move speed)");

                        if (SneakPerformance.GetCollectedStats(MS_MULTI)
                            ?.ToList() is List<StatCollectorEntry> mSEntries)
                        {
                            SB.Append("Sources:").AppendLine();
                            int count = mSEntries.Count;
                            for (int i = 0; i < count; i++)
                            {
                                float mSMulti = mSEntries[i].GetMulti();
                                bool mSMultiGood = mSMulti >= 0;
                                string mSMultiAmount = (mSMulti -1f).Signed();
                                string mSShiftAmount = GetMoveSpeedShiftAmount(Object, mSMulti).Signed();
                                SB.Append(mSEntries[i].Source + ": ")
                                    .AppendColored(mSMultiGood ? "g" : "r", mSMultiAmount).Append("X ")
                                    .Append("(").AppendColored(mSMultiGood ? "g" : "r", mSShiftAmount).Append(" MS)");
                                if (i < count - 1)
                                    SB.AppendLine();
                            }
                        }
                    }
                    if (IsQuicknessMultiplierApplied)
                    {
                        if (!SB.IsNullOrEmpty())
                            SB.AppendLine().AppendLine();

                        float totalQNMulti = GetQuicknessMultiplier();
                        bool qNTotalGood = totalQNMulti >= 1f;
                        SB.Append("Acts at ").AppendColored(qNTotalGood ? "g" : "r", totalQNMulti.ToString()).Append("X the normal speed. ")
                            .Append("(").AppendColored(qNTotalGood ? "g" : "r", AppliedQuicknessMultiplierAmount.Signed()).Append(" quickness)");

                        if (SneakPerformance.GetCollectedStats(QN_MULTI)
                            ?.ToList() is List<StatCollectorEntry> qNEntries)
                        {
                            SB.Append("Sources:").AppendLine();
                            int count = qNEntries.Count;
                            for (int i = 0; i < count; i++)
                            {
                                float qNMulti = qNEntries[i].GetMulti();
                                bool qNMultiGood = qNMulti >= 0;
                                string qNMultiAmount = (qNMulti - 1f).Signed();
                                string qNShiftAmount = GetQuicknessShiftAmount(Object, qNMulti).Signed();
                                SB.Append(qNEntries[i].Source + ": ")
                                    .AppendColored(qNMultiGood ? "g" : "r", qNMultiAmount).Append("X ")
                                    .Append("(").AppendColored(qNMultiGood ? "g" : "r", qNShiftAmount).Append(" QN)");
                                if (i < count - 1)
                                    SB.AppendLine();
                            }
                        }
                    }
                    E.Add(this, SB.ToString());
                }
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(CommandEvent E)
        {
            if (CommandEventsToConceal.ContainsKey(E.Command))
            {
                Sneak.TryConcealAction(
                    Hider: E.Actor,
                    Performance: SneakPerformance,
                    ConcealedAction: CommandEventsToConceal[E.Command]
                        ?.SetEvent(E));
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(EnteredCellEvent E)
        {
            Sneak.TryConcealAction(
                Hider: E.Actor,
                Performance: SneakPerformance,
                ConcealedAction: new ConcealedMinAction<EnteredCellEvent>(
                    E: E,
                    Aggressive: false,
                    Description: !E.Forced ? "sneaking around" : "being knocked around")
                {
                    IAlert.GetAlert<Visual>(Intensity: 10),
                    IAlert.GetAlert<Auditory>(Intensity: 10),
                    IAlert.GetAlert<Olfactory>(Intensity: 8),
                }.Initialize());
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(IsMoveSpeedMultiplierApplied), IsMoveSpeedMultiplierApplied);
            E.AddEntry(this, nameof(AppliedMoveSpeedMultiplierAmount), AppliedMoveSpeedMultiplierAmount);
            E.AddEntry(this, SneakPerformance.EntriesDebugString(out string performanceEntriesContents), performanceEntriesContents);
            E.AddEntry(this, SneakPerformance.CollectedStatsEntriesDebugString(out string collectedStatsEntriesContents), collectedStatsEntriesContents);
            return base.HandleEvent(E);
        }
        public override bool Render(RenderEvent E)
        {
            if (Duration > 0)
            {
                if (!IsBeingPerceived)
                    E.ApplyColors("&K", "w", int.MaxValue, int.MaxValue);
                else
                    E.ApplyColors("&K", "W", int.MaxValue, int.MaxValue);

                E.RenderEffectIndicator("?", null, "&K", "K", 35);
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
                        RecalcStatMultipliers();
                }

            return base.FireEvent(E);
        }

        #endregion
    }
}
