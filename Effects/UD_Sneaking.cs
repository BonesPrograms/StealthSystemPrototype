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

        public const string MS_NAME = "MoveSpeed";
        public const string QN_NAME = "Speed";

        public static Dictionary<string, ConcealedCommandAction> CommandEventsToConceal => new()
        {
            { Survival_Camp.COMMAND_NAME, new ConcealedCommandAction("camping", false, "making camp") {
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
            List<GameObject> witnesses = stealthHelperPart.Witnesses;
            if (!BeforeSneakEvent.Check(Object, SneakPerformance, ref witnesses, ref abortedByEventMessage))
            {
                if (!abortedByEventMessage.IsNullOrEmpty())
                    return Object.ShowFailure(abortedByEventMessage);
                return false;
            }
            stealthHelperPart.Witnesses = witnesses;

            if (Object.HasEffect<UD_Sneaking>()
                && !Object.CanChangeMovementMode(Sneak.VERBING)
                && !Object.FireEvent(Event.New(nameof(Apply) + nameof(UD_Sneaking), "Effect", this)))
                return false;

            Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_movementBuff");
            StatShifter.DefaultDisplayName = DisplayName;
            Object.MovementModeChanged(Sneak.VERBING);
            DidX("begin", Sneak.VERBING, "!");
            RecalcStatMultipliers();
            return true;
        }

        public override void Remove(GameObject Object)
        {
            DidX("stop", Sneak.VERBING);
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
            || ID == GetAttackerHitDiceEvent.ID
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

                        string mSTotalColor = "C";
                        if (totalMSMulti > 1f)
                            mSTotalColor = "G";
                        else
                        if (totalMSMulti < 1f)
                            mSTotalColor = "R";

                        SB.Append("Moves at ").AppendColored(mSTotalColor, totalMSMulti.ToString()).Append("X the normal speed. ")
                            .Append("(").AppendColored(mSTotalColor, AppliedMoveSpeedMultiplierAmount.Signed()).Append(" move speed)");

                        if (SneakPerformance.GetCollectedStats(MS_MULTI)
                            ?.ToList() is List<StatCollectorEntry> mSEntries)
                        {
                            SB.AppendLine()
                                .Append("Sources:")
                                .AppendLine();
                            int count = mSEntries.Count;
                            for (int i = 0; i < count; i++)
                            {
                                float mSMulti = mSEntries[i].GetMulti();
                                string mSMultiAmount = (mSMulti -1f).Signed();
                                string mSShiftAmount = GetMoveSpeedShiftAmount(Object, mSMulti).Signed();
                                bool mSMultiGood = (mSMulti - 1f) >= 0;
                                string mSMultiColor = mSMultiGood ? "G" : "R";
                                SB.Append(mSEntries[i].Source + ": ")
                                    .AppendColored(mSMultiColor, mSMultiAmount).Append("X ")
                                    .Append("(").AppendColored(mSMultiColor, mSShiftAmount).Append(" MS)");
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

                        string qNTotalColor = "C";
                        if (totalQNMulti > 1f)
                            qNTotalColor = "G";
                        else
                        if (totalQNMulti < 1f)
                            qNTotalColor = "R";

                        SB.Append("Acts at ").AppendColored(qNTotalColor, totalQNMulti.ToString()).Append("X the normal speed. ")
                            .Append("(").AppendColored(qNTotalColor, AppliedQuicknessMultiplierAmount.Signed()).Append(" quickness)");

                        if (SneakPerformance.GetCollectedStats(QN_MULTI)
                            ?.ToList() is List<StatCollectorEntry> qNEntries)
                        {
                            SB.AppendLine()
                                .Append("Sources:")
                                .AppendLine();
                            int count = qNEntries.Count;
                            for (int i = 0; i < count; i++)
                            {
                                float qNMulti = qNEntries[i].GetMulti();
                                string qNMultiAmount = (qNMulti - 1f).Signed();
                                string qNShiftAmount = GetQuicknessShiftAmount(Object, qNMulti).Signed();
                                bool qNMultiGood = (qNMulti - 1f) >= 0;
                                string qNMultiColor = qNMultiGood ? "G" : "R";
                                SB.Append(qNEntries[i].Source + ": ")
                                    .AppendColored(qNMultiColor, qNMultiAmount).Append("X ")
                                    .Append("(").AppendColored(qNMultiColor, qNShiftAmount).Append(" QN)");
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
                        ?.SetEvent(E)
                        ?.Initialize(),
                    AlertObject: E.Actor,
                    AlertCell: E.Actor.CurrentCell);
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
                    Action: "moving",
                    Aggressive: false,
                    Description: !E.Forced ? "sneaking around" : "being knocked around")
                {
                    BaseAlert.GetAlert<Visual>(Intensity: 10),
                    BaseAlert.GetAlert<Auditory>(Intensity: 10),
                    BaseAlert.GetAlert<Olfactory>(Intensity: 8),
                }.Initialize(),
                AlertObject: E.Actor,
                AlertCell: E.Actor.CurrentCell);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetAttackerHitDiceEvent E)
        {
            Sneak.TryConcealAction(
                Hider: E.Attacker,
                Performance: SneakPerformance,
                ConcealedAction: new ConcealedMeleeAttackAction(
                    E: E,
                    Description: "attacking =subject.refname= with =object.t=")
                {
                    BaseAlert.GetAlert<Kinesthetic>(Intensity: 35,
                        Properties: new()
                        {
                            { "Pain", null }
                        }),
                    BaseAlert.GetAlert<Visual>(Intensity: 25),
                    BaseAlert.GetAlert<Auditory>(Intensity: 25),
                    BaseAlert.GetAlert<Psionic>(Intensity: 15,
                        Properties: new() 
                        { 
                            { "Intent", "Negative" }
                        }),
                }.Initialize(),
                AlertObject: E.Attacker,
                AlertCell: E.Defender.CurrentCell);
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(GetDebugInternalsEvent E)
        {
            E.AddEntry(this, nameof(IsMoveSpeedMultiplierApplied), IsMoveSpeedMultiplierApplied);
            E.AddEntry(this, nameof(AppliedMoveSpeedMultiplierAmount), AppliedMoveSpeedMultiplierAmount);
            E.AddEntry(
                FX: this,
                Name: SneakPerformance.EntriesDebugString(out string performanceEntriesContents),
                Value: performanceEntriesContents);
            E.AddEntry(
                FX: this,
                Name: SneakPerformance.CollectedStatsEntriesDebugString(out string collectedStatsEntriesContents),
                Value: collectedStatsEntriesContents);
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

                E.RenderEffectIndicator("\u0001", null, "&K", "K", 35);
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
