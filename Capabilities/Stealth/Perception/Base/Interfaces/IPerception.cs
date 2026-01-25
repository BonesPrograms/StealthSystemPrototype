using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using System.Reflection;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s.
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

        /// <summary>
        /// Called once inside the <see cref="IPerception"/>'s default constructor.
        /// </summary>
        /// <remarks>
        /// Override only to make common initialization assignments for derived types.
        /// </remarks>
        public void Construct();

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is first added into the rack.
        /// </summary>
        public void Attach();

        /// <summary>
        /// Called once by a <see cref="PerceptionRack"/> when an <see cref="IPerception"/> is removed from the rack.
        /// </summary>
        public void Remove();

        /// <summary>
        /// Creates deep copy of an <see cref="IPerception"/>, with all the same values as the original.
        /// </summary>
        /// <remarks>
        /// Override this method to null any reference type members that shouldn't be sharing a reference.
        /// </remarks>
        /// <param name="Owner">The new <see cref="GameObject"/> for whom the deep copy is intended.</param>
        /// <returns>A new <see cref="IPerception"/> with values matching the original, and reassigned reference members.</returns>
        public IPerception DeepCopy(GameObject Owner);

        public void AssignDefaultPurview(int? Value = null, string Attributes = null);

        public string ToString(bool Short);

        /// <summary>
        /// Produces an ID-like name for the <see cref="IPerception"/>
        /// </summary>
        /// <param name="Short"></param>
        /// <returns></returns>
        public string GetName(bool Short = false);

        public bool CanPerceiveAlert(IAlert Alert);

        public bool CanPerceive(AlertContext Context)
            => Context?.Alert is IAlert alert
            && CanPerceiveAlert(alert);

        public bool TryPerceive(AlertContext AlertContext);

        public IDetection RaiseDetection(AlertContext AlertContext);

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
