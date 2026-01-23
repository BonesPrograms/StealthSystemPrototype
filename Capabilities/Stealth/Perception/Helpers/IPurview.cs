using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using StealthSystemPrototype.Perceptions;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="BaseConcealedAction"/> occured within proximity of an <see cref="IPerception"/>
    /// </summary>
    public interface IPurview
        : IComposite
        , IComparable<IPurview>
        , IEquatable<IPurview>
    {
        public static int MIN_VALUE => 0;

        public static int MAX_VALUE => 84;

        public IPerception ParentPerception { get; set; }

        public int Value { get; }

        public int EffectiveValue { get; }

        string Attributes { get; }

        public List<string> GetPerviewAttributes();

        public bool HasAttribute(string Attribute);

        public bool HasAttributes(params string[] Attributes);

        public int GetPurviewAdjustment(IPerception ParentPerception, int Value = 0);

        public int GetEffectiveValue();

        public bool IsWithin(IConcealedAction ConcealedAction);

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            string Attributes)
        {
            Writer.WriteOptimized(Value);
            Writer.WriteOptimized(Attributes);
        }
        public static void WriteOptimized(SerializationWriter Writer, IPurview Purview)
            => WriteOptimized(Writer, Purview.Value, Purview.Attributes);

        public static void ReadOptimizedPurview(
            SerializationReader Reader,
            out int Value,
            out string Attributes)
        {
            Value = Reader.ReadOptimizedInt32();
            Attributes = Reader.ReadOptimizedString();
        }
    }
}
