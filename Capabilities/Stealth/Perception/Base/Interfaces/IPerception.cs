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
        public static int MIN_LEVEL => 0;

        public static int MAX_LEVEL => 999;

        public string Name => GetName();

        public string ShortName => GetName(true);

        public GameObject Owner { get; set; }

        public PerceptionRack Rack { get; }

        public IPurview Purview { get; set; }

        public int Level { get; set; }

        public int EffectiveLevel { get; }

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
        #region Static Methods

        public static void RollSave(
            out int NaturalRoll,
            out int Roll,
            out int Difficulty,
            ref bool IgnoreNatural1,
            ref bool IgnoreNatural20,
            AlertContext Context,
            AlertRating AlertRating,
            int BaseDifficulty,
            string Alert,
            string Action,
            GameObject AlertObject,
            bool LogRoll = false)
        {
            GameObject perceiver = Context.Perceiver;

            string perceptionName = Context.Perception.Name;


            GameObject Hider = Context.Hider;

            NaturalRoll = Stat.Random(1, 20);
            Roll = NaturalRoll;
            Difficulty = BaseDifficulty + AlertRating.Rating;

            Roll += Context.Intensity;
            Difficulty += Hider.StatMod(Alert ?? perceptionName);

            ModifyAttackingSaveEvent.Process(
                Attacker: Hider,
                Defender: perceiver,
                Source: AlertObject,
                Stat: perceptionName,
                AttackerStat: Alert,
                Vs: Action,
                NaturalRoll: NaturalRoll,
                Roll: ref Roll,
                BaseDifficulty: BaseDifficulty,
                Difficulty: ref Difficulty,
                IgnoreNatural1: ref IgnoreNatural1,
                IgnoreNatural20: ref IgnoreNatural20,
                Actual: true);

            ModifyOriginatingSaveEvent.Process(
                Attacker: Hider,
                Defender: perceiver,
                Source: AlertObject,
                Stat: perceptionName,
                AttackerStat: Alert,
                Vs: Action,
                NaturalRoll: NaturalRoll,
                Roll: ref Roll,
                BaseDifficulty: BaseDifficulty,
                Difficulty: ref Difficulty,
                IgnoreNatural1: ref IgnoreNatural1,
                IgnoreNatural20: ref IgnoreNatural20,
                Actual: true);

            ModifyDefendingSaveEvent.Process(
                    Attacker: Hider,
                    Defender: perceiver,
                    Source: AlertObject,
                    Stat: perceptionName,
                    AttackerStat: Alert,
                    Vs: Action,
                    NaturalRoll: NaturalRoll,
                    Roll: ref Roll,
                    BaseDifficulty: BaseDifficulty,
                    Difficulty: ref Difficulty,
                    IgnoreNatural1: ref IgnoreNatural1,
                    IgnoreNatural20: ref IgnoreNatural20,
                    Actual: true);
            if (perceiver.IsPlayer())
            {
                switch (perceptionName)
                {
                    case "Intelligence":
                    case "Ego":
                    case "Willpower":
                        perceiver.PlayWorldSound("sfx_ability_mutation_mental_generic_save");
                        break;
                    default:
                        perceiver.PlayWorldSound("sfx_ability_mutation_physical_generic_save");
                        break;
                }
            }
            if (!LogRoll || !XRL.UI.Options.DebugSavingThrows)
            {
                return;
            }
            StringBuilder stringBuilder = Event.NewStringBuilder();
            stringBuilder.Append(perceiver.IsPlayer() ? "Player" : perceiver.Blueprint).Append(" rolled ").Append(NaturalRoll);
            if (Roll != NaturalRoll)
            {
                stringBuilder.Append(" modified to ").Append(Roll);
            }
            stringBuilder.Append(" on ").Append(perceptionName).Append(" save");
            if (Action != null)
            {
                stringBuilder.Append(" vs. ").Append(Action);
            }
            if (Hider != null)
            {
                stringBuilder.Append(" from ").Append(Hider.IsPlayer() ? "player" : Hider.Blueprint);
                if (Alert != null && Alert != perceptionName)
                {
                    stringBuilder.Append(" (using ").Append(Alert).Append(')');
                }
            }
            stringBuilder.Append(" with difficulty ").Append(BaseDifficulty);
            if (Difficulty != BaseDifficulty)
            {
                stringBuilder.Append(" modified to ").Append(Difficulty);
            }
            MessageQueue.AddPlayerMessage(stringBuilder.ToString());
        }
        public static bool MakeSave(
            out int SuccessMargin,
            out int FailureMargin,
            AlertContext Context,
            AlertRating AlertRating,
            string Perception,
            int BaseDifficulty,
            string Alert,
            string Action,
            GameObject AlertObject,
            bool IgnoreNaturals = false,
            bool IgnoreNatural1 = false,
            bool IgnoreNatural20 = false,
            bool IgnoreGodmode = false)
        {
            GameObject Perceiver = Context.Perceiver;

            GameObject Hider = Context.Hider;

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
                AlertRating,
                BaseDifficulty,
                Alert,
                Action,
                AlertObject,
                LogRoll: false);

            bool flag = (Perceiver.IsPlayer() && The.Core.IDKFA && !IgnoreGodmode) || (NaturalRoll == 20 && !IgnoreNatural20) || ((NaturalRoll != 1 || IgnoreNatural1) && Roll >= Difficulty);
            if (flag)
            {
                if (Roll > Difficulty)
                {
                    SuccessMargin = Roll - Difficulty;
                }
            }
            else if (Roll < Difficulty)
            {
                FailureMargin = Difficulty - Roll;
            }
            if (XRL.UI.Options.DebugSavingThrows)
            {
                StringBuilder stringBuilder = Event.NewStringBuilder();
                stringBuilder.Append(Perceiver.DebugName).Append(flag ? " made " : " failed ").Append(Perception)
                    .Append(" save");
                if (Action != null)
                {
                    stringBuilder.Append(" vs. ").Append(Action);
                }
                if (Hider != null)
                {
                    stringBuilder.Append(" from ").Append(Hider.DebugName);
                    if (Alert != null && Alert != Perception)
                    {
                        stringBuilder.Append(" (using ").Append(Alert).Append(')');
                    }
                }
                if (AlertObject != null)
                {
                    stringBuilder.Append(" via ").Append(AlertObject.DebugName);
                }
                stringBuilder.Append(" on ");
                if ((NaturalRoll == 1 && !IgnoreNatural1) || (NaturalRoll == 20 && !IgnoreNatural20))
                {
                    stringBuilder.Append("natural ");
                }
                stringBuilder.Append(NaturalRoll);
                if (Roll != NaturalRoll)
                {
                    stringBuilder.Append(" modified to ").Append(Roll);
                }
                stringBuilder.Append(" with difficulty ").Append(BaseDifficulty);
                if (Difficulty != BaseDifficulty)
                {
                    stringBuilder.Append(" modified to ").Append(Difficulty);
                }
                if (Perceiver.IsPlayer() && The.Core.IDKFA && !IgnoreGodmode)
                {
                    stringBuilder.Append(" (godmode)");
                }
                MessageQueue.AddPlayerMessage(stringBuilder.ToString());
            }
            return flag;
        }

        #endregion
        #region Contracts

        public void ApplyRegistrar(GameObject Object, bool Active = false);

        public void ApplyUnregistrar(GameObject Object, bool Active = false);

        public void RegisterActive(GameObject Object, IEventRegistrar Registrar);

        public void Register(GameObject Object, IEventRegistrar Registrar);

        public bool FireEvent(Event E);

        /// <summary>
        /// Called once inside the <see cref="IPerception"/>'s default constructor.
        /// </summary>
        /// <remarks>
        /// Override only to make common initialization assignments for derived types.
        /// </remarks>
        public void Construct();

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

        public bool CheckInPurview(AlertContext Context);

        public bool CanPerceiveAlert(IAlert Alert);

        public bool CanPerceive(AlertContext Context)
            => Context?.Alert is IAlert alert
            && CanPerceiveAlert(alert);

        public bool TryPerceive(AlertContext Context);

        public IOpinionDetection RaiseDetection(AlertContext Context);

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
