using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;
using XRL.Collections;
using XRL.Messages;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

using SerializeField = UnityEngine.SerializeField;

namespace StealthSystemPrototype.Perceptions
{
    [HasModSensitiveStaticCache]
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BasePerception
        : IPerception
        , IComparable<BasePerception>
    {
        #region Debug
        [UD_DebugRegistry]
        public static void BasePerception_DoDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Perceptions.BasePerception),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(ToString), false },
                });
        }
        #endregion
        #region Const & Static

        public static void RollSave(
            out int NaturalRoll,
            out int Roll,
            out int Difficulty,
            out int BaseDifficulty,
            ref bool IgnoreNatural1,
            ref bool IgnoreNatural20,
            IPerception Perception,
            IPurview Purview,
            ref PerceptionSet.AlertEvent E,
            bool LogRoll = false)
        {
            GameObject perceiver = E.Perceiver;
            GameObject sneaker = E.Sneaker;
            GameObject alertObject = E.AlertObject;

            string perceptionName = Perception.Name;
            string alertName = E.ActionAlert.Name;

            string action = E.Action;

            NaturalRoll = Stat.Random(1, 20);
            Roll = NaturalRoll;

            BaseDifficulty = E.SneakAlert.Intensity;
            Difficulty = BaseDifficulty - E.ActionAlert.Intensity;

            Roll += Purview.GetEffectiveLevel(Perception, ref E);

            ModifyAttackingSaveEvent.Process(
                Attacker: sneaker,
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
                Attacker: sneaker,
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
                Attacker: sneaker,
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
                if (E.ActionAlert.Type.EqualsAny(
                    args: new Type[]
                    {
                        typeof(Psionic),
                        typeof(Visual),
                        typeof(Auditory),
                    }))
                    perceiver.PlayWorldSound("sfx_ability_mutation_mental_generic_save");
                else
                if (E.ActionAlert.Type.EqualsAny(
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

            if (sneaker != null)
            {
                SB.Append(" from ").Append(sneaker.IsPlayer() ? "player" : sneaker.Blueprint);

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
            IPerception Perception,
            IPurview Purview,
            ref PerceptionSet.AlertEvent E,
            bool IgnoreNaturals = false,
            bool IgnoreNatural1 = false,
            bool IgnoreNatural20 = false,
            bool IgnoreGodmode = false)
        {
            GameObject perceiver = E.Perceiver;
            GameObject sneaker = E.Sneaker;
            GameObject alertObject = E.AlertObject;

            string perceptionName = Perception.Name;
            string alertName = E.ActionAlert.Name;

            string action = E.Action;

            if (IgnoreNaturals)
            {
                IgnoreNatural1 = true;
                IgnoreNatural20 = true;
            }
            SuccessMargin = 0;
            FailureMargin = 0;

            RollSave(
                NaturalRoll: out int NaturalRoll,
                Roll: out int Roll,
                Difficulty: out int Difficulty,
                BaseDifficulty: out int BaseDifficulty,
                IgnoreNatural1: ref IgnoreNatural1,
                IgnoreNatural20: ref IgnoreNatural20,
                Perception: Perception,
                Purview: Purview,
                E: ref E,
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

                if (sneaker != null)
                {
                    SB.Append(" from ").Append(sneaker.DebugName);

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
        #region Instance Fields & Properties

        [SerializeField]
        private string _Name;
        public string Name => _Name ??= (GetType()?.ToStringWithGenerics() ?? "null?");

        [SerializeField]
        private string _ShortName;
        public string ShortName => _ShortName ??= (GetType()?.ToStringWithGenerics(true) ?? "?");

        [NonSerialized]
        protected GameObject _Perceiver;
        public virtual GameObject Perceiver
        {
            get => _Perceiver;
            set => _Perceiver = value;
        }

        public PerceptionSet ParentSet => Perceiver?.GetPerceptions();

        public abstract Type AlertType { get; }

        protected int _BaseLevel;
        public virtual int BaseLevel
        {
            get => _BaseLevel;
            set => _BaseLevel = value; 
        }

        [NonSerialized]
        protected int? _Level;
        public int Level
        {
            get
            {
                if (_Level == null
                    && !GettingLevelAdjustment)
                {
                    GettingLevelAdjustment.Toggle();

                    _Level = BaseLevel + GetLevelAdjustment();

                    GettingLevelAdjustment.Toggle();
                }
                return _Level ?? BaseLevel;
            }
        }
        private bool GettingLevelAdjustment = false;

        protected int _BasePurview;
        public int BasePurview
        {
            get => _BasePurview;
            set => _BasePurview = value;
        }

        [NonSerialized]
        protected IPurview _Purview;
        public virtual IPurview Purview
        {
            get => throw new NotSupportedException("Derived classes must provide an implementation.");
            protected set => _Purview = value;
        }

        [NonSerialized]
        protected int _Cooldown;
        public virtual int Cooldown
        {
            get => _Cooldown.Clamp(0, MaxCooldown);
            set => _Cooldown = value.Clamp(0, MaxCooldown);
        }

        public virtual int MaxCooldown => 10;

        #endregion
        #region Constructors

        public BasePerception()
        {
            _Name = null;
            _ShortName = null;

            Perceiver = null;

            BaseLevel = 0;
            _Level = null;

            _Purview = null;

            Cooldown = 0;
        }
        public BasePerception(GameObject Perceiver)
            : this()
        {
            this.Perceiver = Perceiver;
        }
        public BasePerception(
            GameObject Perceiver,
            int BaseLevel)
            : this(Perceiver)
        {
            this.BaseLevel = BaseLevel;
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteGameObject(Perceiver);
            Writer.WriteOptimized(BaseLevel);
            Writer.WriteOptimized(Cooldown);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Perceiver = Reader.ReadGameObject();
            BaseLevel = Reader.ReadOptimizedInt32();
            Cooldown = Reader.ReadOptimizedInt32();
        }

        public virtual void FinalizeRead(SerializationReader Reader)
        {
        }

        #endregion

        #region Base Methods

        /// <summary>
        /// Called once by a <see cref="PerceptionSet"/> when an <see cref="IPerception"/> is first coalesced into the set.
        /// </summary>
        public virtual void Attach()
        {
        }

        /// <summary>
        /// Called once by a <see cref="PerceptionSet"/> when an <see cref="IPerception"/> is removed from the set.
        /// </summary>
        public virtual void Remove()
        {
        }

        /// <summary>
        /// Creates a deep copy of a <see cref="BasePerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Perceiver">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="BasePerception"/> with values matching the original, and reassigned reference members.</returns>
        public virtual IPerception DeepCopy(GameObject Perceiver)
        {
            var perception = Activator.CreateInstance(GetType()) as BasePerception;

            var fields = GetType().GetFields();

            foreach (var fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perception, fieldInfo.GetValue(this));

            perception.Perceiver = Perceiver;
            perception.Purview = null;
            perception.ClearCaches();

            return perception;
        }

        /// <summary>
        /// Creates a deep copy of an <see cref="IPerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="IPerception"/> with values matching the original, and reassigned reference members.</returns>
        IPerception IPerception.DeepCopy(GameObject Owner)
            => DeepCopy(Owner);

        #endregion

        public virtual int GetLevelAdjustment()
            => AdjustTotalPerceptionLevelEvent.GetFor(Perceiver, this, BaseLevel);

        public virtual string ToString(bool Short)
        {
            using var indent = new Indent(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(Short), Short),
                });

            return (Short ? ShortName : Name) + 
                "(" + BaseLevel + "/" + Level + "):" +
                "@P:" + (_Purview?.ToString() ?? "NO_PURVIEW[-0]");
        }

        public override string ToString()
            => ToString(false);

        public virtual bool SameAs(IPerception Other)
            => SameAlertAs(Other);

        public virtual bool SameAlertAs(IPerception Other)
            => AlertType == Other.AlertType;

        public virtual bool CheckInPurview(ref PerceptionSet.AlertEvent E)
        {
            using var indent = new Indent(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Perceiver), Perceiver?.MiniDebugName() ?? "null"),
                    Debug.Arg(nameof(E.Sneaker), E.Sneaker?.MiniDebugName() ?? "null"),
                });

            return Purview?.CheckWithin(this, ref E) ?? false;
        }

        public virtual bool IsOnCooldown()
            => Cooldown > 0;

        public virtual void TickCooldown()
            => (--Cooldown).Clamp(0, MaxCooldown);

        public virtual void GoOnCooldown(int Cooldown)
            => this.Cooldown = Cooldown.Clamp(0, MaxCooldown);

        public virtual void GoOnCooldown()
            => Cooldown = MaxCooldown;

        public virtual void GoOffCooldown()
            => Cooldown = 0;

        public virtual bool CanPerceive(IAlert Alert)
            => Alert?.IsType(AlertType) ?? false;

        public bool CanPerceive(ref PerceptionSet.AlertEvent E)
            => CanPerceive(E.ActionAlert);

        public virtual bool RollPerception(IPurview Purview, ref PerceptionSet.AlertEvent E, out int SuccessMargin, out int FailureMargin)
        {
            SuccessMargin = 0;
            FailureMargin = 0;

            if (!Validate())
                return false;

            if (!CanPerceive(E.ActionAlert))
                return false;

            if (IsOnCooldown())
                return false;

            if (!CheckInPurview(ref E))
                return false;

            bool madeSave = MakeSave(
                SuccessMargin: out SuccessMargin,
                FailureMargin: out FailureMargin,
                Perception: this,
                Purview: Purview,
                E: ref E);

            if (!madeSave)
            {
                GoOnCooldown(FailureMargin);
                return false;
            }
            return true;
        }

        public virtual IOpinionDetection RaiseDetection(ref PerceptionSet.AlertEvent E, int SuccessMargin)
        {
            using var indent = new Indent(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ToString()),
                    Debug.Arg(nameof(E.Perceiver), E.Perceiver.MiniDebugName()),
                    Debug.Arg(nameof(E.Sneaker), E.Sneaker.MiniDebugName()),
                });

            var level = AwarenessLevel.Aware;

            if (SuccessMargin >= 10)
                level++;
            if (SuccessMargin >= 20)
                level = AwarenessLevel.Alert;

            var detectionEvent = IOpinionDetection.GetDetectionEvent(ref E, level);

            return Perceiver.Brain.AddOpinionDetection(
                Detection: GetDetectionOpinionEvent.GetFor(
                    Perceiver: Perceiver,
                    Sneaker: E.Sneaker,
                    Detection: new Curious(),
                    Level: ref level),
                E: ref detectionEvent);
        }

        public virtual void ClearCaches()
        {
            _Name = null;
            _ShortName = null;
            _Level = null;
            _Purview = null;
        }

        public virtual bool Validate()
            => Perceiver != null;

        #region Event Handling

        public virtual bool WantEvent(int ID, int Cascade)
            => false
            || ID == EnteredCellEvent.ID
            ;
        public virtual bool HandleEvent(EnteredCellEvent E)
        {
            ClearCaches();
            return true;
        }

        #endregion
        #region Virtual HandleEvent

        public virtual bool HandleEvent(AdjustTotalPerceptionLevelEvent E)
            => true;

        public virtual bool HandleEvent(AdjustTotalPurviewEvent E)
            => true;

        public virtual bool HandleEvent(BeforeSneakEvent E)
            => true;

        public virtual bool HandleEvent(TryConcealActionEvent E)
            => true;

        #endregion
        #region Comparison

        public int CompareLevelTo(BasePerception Other)
            => BaseLevel - Other.BaseLevel;

        public int CompareEffectiveLevelTo(BasePerception Other)
            => Level - Other.Level;

        public int ComparePurviewTo(BasePerception Other)
            => Purview.BaseValue.CompareTo(Other.Purview.BaseValue);

        public virtual int CompareTo(BasePerception Other)
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

        public int CompareBaseLevelTo(IPerception Other)
            => BaseLevel - Other.BaseLevel;

        public int CompareLevelTo(IPerception Other)
            => Level - Other.Level;

        public int ComparePurviewTo(IPerception Other)
            => Purview.BaseValue.CompareTo(Other.Purview.BaseValue);

        public virtual int CompareTo(IPerception Other)
        {
            if (EitherNull(this, Other, out int comparison))
                return comparison;

            int levelComp = CompareBaseLevelTo(Other);
            if (levelComp != 0)
                return levelComp;

            int effectiveLevelComp = CompareLevelTo(Other);
            if (effectiveLevelComp != 0)
                return effectiveLevelComp;

            return ComparePurviewTo(Other);
        }

        #endregion
    }
}
