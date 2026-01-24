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

        public IPurview ReadPurview(SerializationReader Reader, IPerception ParentPerception);

        public static void WriteOptimized(
            SerializationWriter Writer,
            GameObject Owner,
            int Level,
            IPurview Purview)
        {
            Writer.WriteGameObject(Owner);
            Writer.WriteOptimized(Level);
            IPurview.WriteOptimized(Writer, Purview);
        }
        public static void WriteOptimized(SerializationWriter Writer, IPerception Perception)
            => WriteOptimized(Writer, Perception.Owner, Perception.Level, Perception.Purview);

        public static void ReadOptimizedPurview(
            SerializationReader Reader,
            IPerception Perception,
            out GameObject Owner,
            out int Level,
            out IPurview Purview)
        {
            Owner = Reader.ReadGameObject();
            Level = Reader.ReadOptimizedInt32();
            Purview = Perception.ReadPurview(Reader, Perception);
        }

        #endregion
        #region Contracts

        public string ToString(bool Short);

        public string GetName(bool Short = false);

        public IPurview GetPurview();

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
