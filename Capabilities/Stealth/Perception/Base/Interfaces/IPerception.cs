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

        GameObject Perceiver { get; set; }

        int Level { get; set; }

        #region Serialization

        void FinalizeRead(SerializationReader Reader);

        #endregion
        #region Contracts
        #region Event Registration

        #endregion
        #region Object Life-cycle

        /// <summary>
        /// Called once by a <see cref="PerceptionsSet"/> when an <see cref="IPerception"/> is first coalesced into the set.
        /// </summary>
        void Added();

        /// <summary>
        /// Called once by a <see cref="PerceptionsSet"/> when an <see cref="IPerception"/> is removed from the set.
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
        /// Produces an ID-like name for the <see cref="IPerception"/>.
        /// </summary>
        /// <param name="Short">Indicates an alternate shorter version of the output.<br/><br/>A good option is to use <see cref="Extensions.Acronymize(string)"/> to get an acronym.</param>
        /// <returns>The ID-like name of the <see cref="IPerception"/>.</returns>
        string GetName(bool Short = false);

        GameObject GetPerceiver();

        /// <summary>
        /// Get the <see cref="IAlert"/> <see langword="class"/> <see cref="Type"/> that this <see cref="IPerception"/> utilizes.
        /// </summary>
        /// <returns>The <see cref="IAlert"/> <see langword="class"/> <see cref="Type"/> that this <see cref="IPerception"/> utilizes.</returns>
        Type GetAlertType();

        /// <summary>
        /// Get the <see cref="IPurview"/> used by this <see cref="IPerception"/> to determine whether an <see cref="IConcealedAction"/> is in proximity enough to be detected.
        /// </summary>
        /// <returns>The <see cref="IPurview"/> used by this <see cref="IPerception"/> to determine whether an <see cref="IConcealedAction"/> is in proximity enough to be detected.</returns>
        IPurview GetPurview();

        int GetLevel();

        int GetLevelAdjustment(int Level = 0);

        int GetEffectiveLevel();

        int GetCooldown();

        int GetMaxCoolDown();

        #endregion

        string ToString(bool Short);

        #region Compatibility

        bool SameAs(IPerception Other);

        bool SameAlertAs(IPerception Other);

        bool IsCompatibleWith(IPurview Purview);

        #endregion
        #region Purview

        /// <summary>
        /// Used to configure the <see cref="IPurview"/> used by this <see cref="IPerception"/> without having to pass arguments to a constructor.
        /// </summary>
        /// <param name="Value">The value to which the <see cref="IPurview.Value"/> should be set.</param>
        /// <param name="args">An optional set of string &amp; object pairs that represent named values to pass on to <see cref="IPurview.Configure(Dictionary{string, object})"/>.</param>
        void ConfigurePurview(int Value, Dictionary<string, object> args = null);

        bool CheckInPurview(AlertContext Context);

        #endregion
        #region Cooldown

        bool IsOnCooldown();

        void TickCooldown();

        void GoOnCooldown(int Cooldown);

        void GoOnCooldown();

        void GoOffCooldown();

        #endregion
        #region Perceive

        bool CanPerceiveAlert(IAlert Alert);

        bool CanPerceive(AlertContext Context);

        bool TryPerceive(AlertContext Context, out int SuccessMargin, out int FailureMargin);

        IOpinionDetection RaiseDetection(AlertContext Context, int SuccessMargin);

        #endregion

        void ClearCaches();

        bool Validate();

        #endregion
    }
}
