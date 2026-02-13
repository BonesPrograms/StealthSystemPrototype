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
using StealthSystemPrototype.Detetection.ResponseGoals;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="IDetectionResponseGoal"/>s.
    /// </summary>
    public interface IPerception : IComposite
    {
        #region Static & Const

        public static int MIN_LEVEL => 0;

        public static int MAX_LEVEL => 999;

        public static bool IsPerceptionOfAlert<A>(IPerception IPerception)
            where A : class, IAlert, new()
            => IPerception is IAlertTypedPerception<A>;

        #endregion

        /// <summary>ID-like name for the perception.</summary>
        string Name { get; }

        /// <summary>Acronymised version of <see cref="Name"/>.</summary>
        string ShortName { get; }

        /// <summary>
        /// The <see cref="GameObject"/> to whom this perception belongs. 
        /// </summary>
        GameObject Perceiver { get; set; }

        /// <summary>The <see cref="IAlert"/> <see langword="class"/> <see cref="Type"/> that this <see cref="IPerception"/> utilizes.</summary>
        Type AlertType { get; }

        int Level { get; set; }

        int EffectiveLevel => Level + GetLevelAdjustment();

        int Cooldown { get; set; }

        int MaxCooldown { get; }

        #region Serialization

        void FinalizeRead(SerializationReader Reader);

        #endregion
        #region Contracts
        #region Object Life-cycle

        /// <summary>
        /// Called once by a <see cref="PerceptionSet"/> when an <see cref="IPerception"/> is first coalesced into the set.
        /// </summary>
        void AfterAdded();

        /// <summary>
        /// Called once by a <see cref="PerceptionSet"/> when an <see cref="IPerception"/> is removed from the set.
        /// </summary>
        void Remove();

        /// <summary>
        /// Creates a deep copy of an <see cref="IPerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="IPerception"/> with values matching the original, and reassigned reference members.</returns>
        IPerception DeepCopy(GameObject Owner);

        #endregion
        #region Field Accessors

        /// <summary>
        /// Get the <see cref="IPurview"/> used by this <see cref="IPerception"/> to determine whether an <see cref="IConcealedAction"/> is in proximity enough to be detected.
        /// </summary>
        /// <returns>The <see cref="IPurview"/> used by this <see cref="IPerception"/> to determine whether an <see cref="IConcealedAction"/> is in proximity enough to be detected.</returns>
        IPurview GetPurview();

        #endregion

        string ToString(bool Short);

        int GetLevelAdjustment();

        #region Compatibility

        bool SameAs(IPerception Other);

        bool SameAlertAs(IPerception Other);

        bool IsCompatibleWith(IPurview Purview);

        #endregion
        #region Purview

        /// <summary>
        /// Used to configure the <see cref="IPurview"/> used by this <see cref="IPerception"/> without having to pass arguments to a constructor.
        /// </summary>
        /// <param name="Value">The value to which the <see cref="IPurview.BaseValue"/> should be set.</param>
        /// <param name="args">An optional set of string &amp; object pairs that represent named values to pass on to <see cref="IPurview.Configure(Dictionary{string, object})"/>.</param>
        void ConfigurePurview(int Value, Dictionary<string, object> args = null);

        bool CheckInPurview(ref PerceptionSet.AlertEvent E);

        #endregion
        #region Cooldown

        bool IsOnCooldown();

        void TickCooldown();

        void GoOnCooldown(int Cooldown);

        void GoOnCooldown();

        void GoOffCooldown();

        #endregion
        #region Perceive

        bool CanPerceive(IAlert Alert)
            => AlertType == Alert.GetType();

        bool CanPerceive(ref PerceptionSet.AlertEvent E);

        bool RollPerception(ref PerceptionSet.AlertEvent E, out int SuccessMargin, out int FailureMargin);

        IOpinionDetection RaiseDetection(ref PerceptionSet.AlertEvent E, int SuccessMargin);

        #endregion

        void ClearCaches();

        bool Validate();

        #endregion
    }
}
