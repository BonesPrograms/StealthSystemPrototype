using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.Messages;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

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
