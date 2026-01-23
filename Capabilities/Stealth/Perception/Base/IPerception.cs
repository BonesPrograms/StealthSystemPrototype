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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Perceptions
{
    /// <summary>
    /// Contracts a class as capable of detecting <see cref="IConcealedAction"/>s and issuing <see cref="BaseDetection"/>s.
    /// </summary>
    public interface IPerception : IComposite, IComparable<IPerception>
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

        public string GetName(bool Short = false);

        public bool CanPerceive(IAlert Alert);

        public bool Perceive(IAlert Alert);

        public int GetLevelAdjustment(int Level = 0);

        public int GetPurviewAdjustment(int Value = 0);

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
    }
}
