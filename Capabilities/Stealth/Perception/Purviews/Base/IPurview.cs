using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    /// <summary>
    /// Contracts a type as being capable of determining whether or not an <see cref="IConcealedAction"/> occured within proximity of an <see cref="IPerception"/>
    /// </summary>
    public interface IPurview
        : IComposite
        , IComparable<IPurview>
        , IEquatable<IPurview>
    {
        public static int MIN_VALUE => 0;

        public static int MAX_VALUE => 84;

        public IPerception ParentPerception { get; }

        public int Value { get; }

        public int EffectiveValue { get; }

        string Attributes { get; }

        #region Serialization

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

        public void FromReader(SerializationReader Reader, IPerception ParentPerception);

        #endregion
        #region Contracts

        public IPurview SetParentPerception(IPerception ParentPerception);

        public List<string> GetPerviewAttributes();

        public bool HasAttribute(string Attribute);

        public bool HasAttributes(params string[] Attributes);

        public int GetPurviewAdjustment(IPerception ParentPerception, int Value = 0);

        public int GetEffectiveValue();

        public bool IsWithin(Cell Cell);

        public void ClearCaches();

        #endregion
        #region Comparioson

        public new int CompareTo(IPurview Other)
            => EitherNull(this, Other, out int comparison)
            ? comparison
            : (Value - Other.Value) +
                (EffectiveValue - Other.EffectiveValue);

        #endregion
    }
}
