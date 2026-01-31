using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using XRL.Messages;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseOpinionGoal"/>s.
    /// </summary>
    public interface IPerception
        : IComposite
        , IComparable<IPerception>
        , IEventHandler
    {
        #region Static & Const

        public static int MIN_LEVEL => 0;

        public static int MAX_LEVEL => 999;

        public static bool IsPerceptionOfAlert<A>(IPerception IPerception)
            where A : class, IAlert, new()
            => IPerception is IAlertTypedPerception<A>;

        public static void RollSave(
            out int NaturalRoll,
            out int Roll,
            out int Difficulty,
            out int BaseDifficulty,
            ref bool IgnoreNatural1,
            ref bool IgnoreNatural20,
            AlertContext Context,
            bool LogRoll = false)
        {
            GameObject perceiver = Context.Perceiver;
            GameObject hider = Context.Hider;
            GameObject alertObject = Context.AlertObject;

            IPerception perception = Context.Perception;
            IAlert actonAlert = Context.ActionAlert;
            IAlert sneakAlert = Context.SneakAlert;

            string perceptionName = perception.GetName();
            string alertName = actonAlert.Name;

            string action = Context.ParentAction.Action;

            NaturalRoll = Stat.Random(1, 20);
            Roll = NaturalRoll;

            BaseDifficulty = sneakAlert.Intensity;
            Difficulty = BaseDifficulty - actonAlert.Intensity;

            Roll += perception.GetEffectiveLevel();

            ModifyAttackingSaveEvent.Process(
                Attacker: hider,
                Defender: perceiver,
                Source: alertObject,
                Stat: perceptionName,
                AttackerStat: alertName,
                Vs: action,
                NaturalRoll: NaturalRoll,
                Roll: ref Roll,
                BaseDifficulty: BaseDifficulty,
                Difficulty: ref Difficulty,
                IgnoreNatural1: ref IgnoreNatural1,
                IgnoreNatural20: ref IgnoreNatural20,
                Actual: true);

            ModifyOriginatingSaveEvent.Process(
                Attacker: hider,
                Defender: perceiver,
                Source: alertObject,
                Stat: perceptionName,
                AttackerStat: alertName,
                Vs: action,
                NaturalRoll: NaturalRoll,
                Roll: ref Roll,
                BaseDifficulty: BaseDifficulty,
                Difficulty: ref Difficulty,
                IgnoreNatural1: ref IgnoreNatural1,
                IgnoreNatural20: ref IgnoreNatural20,
                Actual: true);

            ModifyDefendingSaveEvent.Process(
                Attacker: hider,
                Defender: perceiver,
                Source: alertObject,
                Stat: perceptionName,
                AttackerStat: alertName,
                Vs: action,
                NaturalRoll: NaturalRoll,
                Roll: ref Roll,
                BaseDifficulty: BaseDifficulty,
                Difficulty: ref Difficulty,
                IgnoreNatural1: ref IgnoreNatural1,
                IgnoreNatural20: ref IgnoreNatural20,
                Actual: true);

            if (perceiver.IsPlayer())
            {
                if (actonAlert.Type.EqualsAny(
                    args: new Type[]
                    {
                        typeof(Psionic),
                        typeof(Visual),
                        typeof(Auditory),
                    }))
                    perceiver.PlayWorldSound("sfx_ability_mutation_mental_generic_save");
                else
                if (actonAlert.Type.EqualsAny(
                    args: new Type[]
                    {
                        typeof(Olfactory),
                        typeof(Thermal),
                        typeof(Kinesthetic),
                    }))
                    perceiver.PlayWorldSound("sfx_ability_mutation_physical_generic_save");
                else
                    perceiver.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_physicalDefect_generic_activate");

            }

            if (!LogRoll
                || !XRL.UI.Options.DebugSavingThrows)
                return;

            StringBuilder SB = Event.NewStringBuilder();
            SB.Append(perceiver.IsPlayer() ? "Player" : perceiver.Blueprint)
                .Append(" rolled ")
                .Append(NaturalRoll);

            if (Roll != NaturalRoll)
                SB.Append(" modified to ").Append(Roll);

            SB.Append(" on ").Append(perceptionName).Append(" save");

            if (action != null)
                SB.Append(" vs. ").Append(action);

            if (hider != null)
            {
                SB.Append(" from ").Append(hider.IsPlayer() ? "player" : hider.Blueprint);

                if (alertName != null && alertName != perceptionName)
                    SB.Append(" (using ").Append(alertName).Append(')');
            }
            SB.Append(" with difficulty ").Append(BaseDifficulty);

            if (Difficulty != BaseDifficulty)
                SB.Append(" modified to ").Append(Difficulty);

            MessageQueue.AddPlayerMessage(SB.ToString());
        }
        public static bool MakeSave(
            out int SuccessMargin,
            out int FailureMargin,
            BasePerception Perception,
            AlertContext Context,
            bool IgnoreNaturals = false,
            bool IgnoreNatural1 = false,
            bool IgnoreNatural20 = false,
            bool IgnoreGodmode = false)
        {
            GameObject perceiver = Context.Perceiver;
            GameObject hider = Context.Hider;
            GameObject alertObject = Context.AlertObject;

            string perceptionName = Perception.GetName();
            string alertName = Context.ActionAlert.Name;

            string action = Context.ParentAction.Action;

            if (IgnoreNaturals)
            {
                IgnoreNatural1 = true;
                IgnoreNatural20 = true;
            }
            SuccessMargin = 0;
            FailureMargin = 0;

            RollSave(
                out int NaturalRoll,
                out int Roll,
                out int Difficulty,
                out int BaseDifficulty,
                ref IgnoreNatural1,
                ref IgnoreNatural20,
                Context,
                LogRoll: false);

            bool godMode = perceiver.IsPlayer()
                && The.Core.IDKFA
                && !IgnoreGodmode;

            bool validNat20 = NaturalRoll == 20
                && !IgnoreNatural20;

            bool validNat1 = NaturalRoll == 1
                && !IgnoreNatural1;

            bool rollSuccess = Roll >= Difficulty;

            bool made;

            if (godMode)
                made = true;
            else
            if (validNat20)
                made = true;
            else
            if (validNat1)
                made = false;
            else
                made = rollSuccess;

            if (made)
            {
                if (Roll > Difficulty)
                    SuccessMargin = Roll - Difficulty;
            }
            else
            if (Roll < Difficulty)
                FailureMargin = Difficulty - Roll;

            if (XRL.UI.Options.DebugSavingThrows)
            {
                StringBuilder SB = Event.NewStringBuilder();
                SB.Append(perceiver.DebugName)
                    .Append(made ? " made " : " failed ")
                    .Append(perceptionName)
                    .Append(" save");

                if (action != null)
                    SB.Append(" vs. ").Append(action);

                if (hider != null)
                {
                    SB.Append(" from ").Append(hider.DebugName);

                    if (alertName != null && alertName != perceptionName)
                        SB.Append(" (using ").Append(alertName).Append(')');
                }
                if (alertObject != null)
                    SB.Append(" via ").Append(alertObject.DebugName);

                SB.Append(" on ");

                if ((NaturalRoll == 1 && !IgnoreNatural1) || (NaturalRoll == 20 && !IgnoreNatural20))
                    SB.Append("natural ");

                SB.Append(NaturalRoll);

                if (Roll != NaturalRoll)
                    SB.Append(" modified to ").Append(Roll);

                SB.Append(" with difficulty ").Append(BaseDifficulty);

                if (Difficulty != BaseDifficulty)
                    SB.Append(" modified to ").Append(Difficulty);

                if (perceiver.IsPlayer() && The.Core.IDKFA && !IgnoreGodmode)
                    SB.Append(" (godmode)");

                MessageQueue.AddPlayerMessage(SB.ToString());
            }
            return made;
        }

        #endregion
        #region Serialization

        public void FinalizeRead(SerializationReader Reader);

        #endregion
        #region Contracts

        #region Event Registration

        public void ApplyRegistrar(GameObject Object, bool Active = false);

        public void ApplyUnregistrar(GameObject Object, bool Active = false);

        public void RegisterActive(GameObject Object, IEventRegistrar Registrar);

        public void Register(GameObject Object, IEventRegistrar Registrar);

        public bool FireEvent(Event E);

        #endregion
        #region Object Life-cycle

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack if indicated as initial.
        /// </summary>
        public void Initialize();

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack.
        /// </summary>
        public void Attach();

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack if indicated as not creation.
        /// </summary>
        public void AddedAfterCreation();

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is removed from the rack.
        /// </summary>
        public void Remove();

        /// <summary>
        /// Creates a deep copy of an <see cref="IPerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="IPerception"/> with values matching the original, and reassigned reference members.</returns>
        public IPerception DeepCopy(GameObject Owner);

        #endregion
        #region Field Accessors

        /// <summary>
        /// Produces an ID-like name for the <see cref="IPerception"/>.
        /// </summary>
        /// <param name="Short">Indicates an alternate shorter version of the output.<br/><br/>A good option is to use <see cref="Extensions.Acronymize(string)"/> to get an acronym.</param>
        /// <returns>The ID-like name of the <see cref="IPerception"/>.</returns>
        public string GetName(bool Short = false);

        public GameObject GetOwner();

        /// <summary>
        /// Get the <see cref="IAlert"/> <see langword="class"/> <see cref="Type"/> that this <see cref="IPerception"/> utilizes.
        /// </summary>
        /// <returns>The <see cref="IAlert"/> <see langword="class"/> <see cref="Type"/> that this <see cref="IPerception"/> utilizes.</returns>
        public Type GetAlertType();

        /// <summary>
        /// Get the <see cref="IPurview"/> used by this <see cref="IPerception"/> to determine whether an <see cref="IConcealedAction"/> is in proximity enough to be detected.
        /// </summary>
        /// <returns>The <see cref="IPurview"/> used by this <see cref="IPerception"/> to determine whether an <see cref="IConcealedAction"/> is in proximity enough to be detected.</returns>
        public IPurview GetPurview();

        public int GetLevel();

        public int GetLevelAdjustment(int Level = 0);

        public int GetEffectiveLevel();

        public int GetCooldown();

        public int GetMaxCoolDown();

        #endregion

        public string ToString(bool Short);

        #region Compatibility

        public bool SameAs(IPerception Other);

        public bool SameAlertAs(IPerception Other);

        public bool IsCompatibleWith(IPurview Purview);

        #endregion
        #region Purview

        /// <summary>
        /// Used to configure the <see cref="IPurview"/> used by this <see cref="IPerception"/> without having to pass arguments to a constructor.
        /// </summary>
        /// <param name="Value">The value to which the <see cref="IPurview.Value"/> should be set.</param>
        /// <param name="args">An optional set of string &amp; object pairs that represent named values to pass on to <see cref="IPurview.Configure(Dictionary{string, object})"/>.</param>
        public void ConfigurePurview(int Value, Dictionary<string, object> args = null);

        public bool CheckInPurview(AlertContext Context);

        #endregion
        #region Cooldown

        public bool IsOnCooldown();

        public void TickCooldown();

        public void GoOnCooldown(int Cooldown);

        public void GoOnCooldown();

        public void GoOffCooldown();

        #endregion
        #region Perceive

        public bool CanPerceiveAlert(IAlert Alert);

        public bool CanPerceive(AlertContext Context);

        public bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin);

        public IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin);

        #endregion

        public void ClearCaches();

        public bool Validate();

        #endregion
        #region Comparison

        public int CompareLevelTo(IPerception Other)
            => GetLevel() - Other.GetLevel();

        public int CompareEffectiveLevelTo(IPerception Other)
            => GetEffectiveLevel() - Other.GetEffectiveLevel();

        public int ComparePurviewTo(IPerception Other)
            => GetPurview().CompareTo(Other.GetPurview());

        public new int CompareTo(IPerception Other)
        {
            if (EitherNull(this, Other, out int comparison))
                return comparison;

            int levelComp = CompareLevelTo(Other);
            if (levelComp != 0)
                return levelComp;

            int effectiveLevelComp = CompareEffectiveLevelTo(Other);
            if (effectiveLevelComp != 0)
                return effectiveLevelComp;

            return ComparePurviewTo(Other);
        }

        #endregion
    }
}
