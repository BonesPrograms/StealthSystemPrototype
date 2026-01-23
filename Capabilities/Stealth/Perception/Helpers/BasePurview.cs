using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [Serializable]
    public class BasePurview
        : IPurview
        , IComposite
    {
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        private IPerception _ParentPerception;
        public IPerception ParentPerception
        {
            get => _ParentPerception;
            set => _ParentPerception = value;
        }

        private int _Value;
        public int Value
        {
            get => _Value;
            protected set => _Value = value;
        }

        private string _Attributes;
        public string Attributes
        {
            get => _Attributes;
            protected set => _Attributes = value;
        }

        public int EffectiveValue => GetEffectiveValue();

        #region Constructors

        protected BasePurview()
        {
            _ParentPerception = null;
            Value = 0;
        }
        public BasePurview(IPerception ParentPerception, int Value, string Attributes)
            : this()
        {
            this.ParentPerception = ParentPerception;
            this.Value = Value;
            this.Attributes = Attributes;
        }
        public BasePurview(BasePurview Source)
            : this(Source.ParentPerception, Source.Value, Source.Attributes)
        {
        }

        #endregion

        public override string ToString()
            => Value + "(E:" + EffectiveValue + ")" + "{" + Attributes + "}";

        public virtual int GetEffectiveValue()
            => Value + GetPurviewAdjustment(ParentPerception, Value);

        public virtual List<string> GetPerviewAttributes()
            => Attributes?.CachedCommaExpansion();

        public virtual int GetPurviewAdjustment(IPerception ParentPerception, int Value = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(ParentPerception.Owner, ParentPerception, Value);

        public BasePurview SetValue(int Value)
        {
            this.Value = Value;
            return this;
        }

        public BasePurview AdjustBy(int Amount)
            => SetValue(Value + Amount);

        #region Predicates

        public virtual bool HasAttribute(string Attribute)
            => GetPerviewAttributes().Contains(Attribute);

        public virtual bool HasAttributes(params string[] Attributes)
            => !Attributes.IsNullOrEmpty()
            && Attributes.All(a => HasAttribute(a));

        public virtual bool IsWithin(IConcealedAction ConcealedAction)
            => false;

        #endregion
        #region Equatable

        public bool Equals(IPurview Other)
            => Utils.EitherNull(this, Other, out bool areEqual)
            ? areEqual
            : Value == Other.Value
                || EffectiveValue == Other.EffectiveValue;

        #endregion
        #region Comparable

        public int CompareTo(IPurview Other)
            => Utils.EitherNull(this, Other, out int comparison)
            ? comparison
            : (Value - Other.Value) +
                (EffectiveValue - Other.EffectiveValue);

        #endregion
        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            string Attributes)
            => IPurview.WriteOptimized(Writer, Value, Attributes);

        public static void WriteOptimized(SerializationWriter Writer, BasePurview Purview)
            => IPurview.WriteOptimized(Writer, Purview);

        public static void ReadOptimizedPurview(
            SerializationReader Reader,
            out int Value,
            out string Attributes)
            => IPurview.ReadOptimizedPurview(Reader, out Value, out Attributes);

        public static BasePurview ReadOptimizedPurview(SerializationReader Reader, IPerception ParentPerception)
        {
            IPurview.ReadOptimizedPurview(Reader, out int value, out string attributes);
            return new(ParentPerception, value, attributes);
        }

        public virtual void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer, Value, Attributes);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ReadOptimizedPurview(Reader, out _Value, out _Attributes);
        }

        #endregion
        #region Conversion

        public static explicit operator int(BasePurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
