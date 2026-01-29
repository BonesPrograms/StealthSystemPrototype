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
        #region Static Methods

        public static void RollSave(
            out int NaturalRoll,
            out int Roll,
            out int Difficulty,
            ref bool IgnoreNatural1,
            ref bool IgnoreNatural20,
            AlertContext Context,
            int BaseDifficulty,
            bool LogRoll = false)
        {
            GameObject perceiver = Context.Perceiver;
            GameObject hider = Context.Hider;
            GameObject alertObject = Context.AlertObject;

            IPerception perception = Context.Perception;
            IAlert alert = Context.Alert;

            string perceptionName = perception.Name;
            string alertName = alert.Name;

            string action = Context.ParentAction.Action;

            NaturalRoll = Stat.Random(1, 20);
            Roll = NaturalRoll;

            Difficulty = BaseDifficulty + (alert.Intensity - Context.AlertConcealment);

            Roll += perception.Level;

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
                if (alert.Type.EqualsAny(
                    args: new Type[]
                    {
                        typeof(Psionic),
                        typeof(Visual),
                        typeof(Auditory),
                    }))
                    perceiver.PlayWorldSound("sfx_ability_mutation_mental_generic_save");
                else
                if (alert.Type.EqualsAny(
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
            AlertContext Context,
            int BaseDifficulty,
            bool IgnoreNaturals = false,
            bool IgnoreNatural1 = false,
            bool IgnoreNatural20 = false,
            bool IgnoreGodmode = false)
        {
            GameObject perceiver = Context.Perceiver;
            GameObject hider = Context.Hider;
            GameObject alertObject = Context.AlertObject;

            string perceptionName = Context.Perception.Name;
            string alertName = Context.Alert.Name;

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
                ref IgnoreNatural1,
                ref IgnoreNatural20,
                Context,
                BaseDifficulty,
                LogRoll: false);

            bool made =
                (perceiver.IsPlayer() 
                    && The.Core.IDKFA && !IgnoreGodmode)
                || (NaturalRoll == 20 
                    && !IgnoreNatural20)
                || ((NaturalRoll != 1 
                        || IgnoreNatural1)
                    && Roll >= Difficulty);

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
        public static int MIN_LEVEL => 0;

        public static int MAX_LEVEL => 999;

        public string Name => GetName();

        public string ShortName => GetName(true);

        public GameObject Owner { get; set; }

        public PerceptionRack Rack { get; }

        public IPurview Purview { get; set; }

        public int Level { get; set; }

        public int EffectiveLevel { get; }

        public int Cooldown { get; set; }

        public int MaxCooldown { get; }

        #region Serialization

        public void FinalizeRead(SerializationReader Reader);

        /// <summary>
        /// Writes the <see cref="IPurview"/> member of an <see cref="IPerception"/> during its serialization. 
        /// </summary>
        /// <remarks>
        /// Assume this will be called during the <see cref="IPerception"/>'s call to <see cref="IComposite.Write"/>.
        /// </remarks>
        /// <param name="Writer">The writer that will do the serialization.</param>
        /// <param name="Purview">The <see cref="IPerception.Purview"/> to be written.</param>
        public void WritePurview(SerializationWriter Writer, IPurview Purview);

        /// <summary>
        /// Reads the <see cref="IPurview"/> member of an <see cref="IPerception"/> during its deserialization. 
        /// </summary>
        /// <remarks>
        /// Assume this will be called during the <see cref="IPerception"/>'s call to <see cref="IComposite.Read"/>.
        /// </remarks>
        /// <param name="Reader">The reader that will do the deserialization.</param>
        /// <param name="Purview">An assignable field or variable from which <see cref="IPerception.Purview"/> can be assigned.</param>
        /// <param name="ParentPerception">The <see cref="IPerception"/> whose <see cref="IPerception.Purview"/> is being read, which should be assigned to its <see cref="IPurview.ParentPerception"/> field.</param>
        public void ReadPurview(SerializationReader Reader, ref IPurview Purview, IPerception ParentPerception = null);

        #endregion
        #region Contracts

        public void ApplyRegistrar(GameObject Object, bool Active = false);

        public void ApplyUnregistrar(GameObject Object, bool Active = false);

        public void RegisterActive(GameObject Object, IEventRegistrar Registrar);

        public void Register(GameObject Object, IEventRegistrar Registrar);

        public bool FireEvent(Event E);

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

        public void AssignDefaultPurview(int Value);

        public IPurview GetDefaultPurview(int Value);

        public string ToString(bool Short);

        /// <summary>
        /// Produces an ID-like name for the <see cref="IPerception"/>
        /// </summary>
        /// <param name="Short"></param>
        /// <returns></returns>
        public string GetName(bool Short = false);

        public bool SameAs(IPerception Other);

        public bool IsOnCooldown()
            => Cooldown > 0;

        public void TickCooldown()
            => (--Cooldown).Clamp(0, MaxCooldown);

        public void GoOnCooldown(int Cooldown)
            => this.Cooldown = Cooldown.Clamp(0, MaxCooldown);

        public void GoOnCooldown()
            => Cooldown = MaxCooldown;

        public void GoOffCooldown()
            => Cooldown = 0;

        public bool CheckInPurview(AlertContext Context);

        public bool CanPerceiveAlert(IAlert Alert);

        public bool CanPerceive(AlertContext Context)
            => Context?.Alert is IAlert alert
            && CanPerceiveAlert(alert);

        public bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin);

        public IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin);

        public int GetLevelAdjustment(int Level = 0);

        public void ClearCaches();

        public bool Validate()
        {
            if (Owner == null)
                return false;

            return true;
        }

        #endregion
        #region Comparison

        public int CompareLevelTo(IPerception Other)
            => Level - Other.Level;

        public int CompareEffectiveLevelTo(IPerception Other)
            => EffectiveLevel - Other.EffectiveLevel;

        public int ComparePurviewTo(IPerception Other)
            => Purview.CompareTo(Other.Purview);

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
