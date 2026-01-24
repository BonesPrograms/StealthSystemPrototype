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
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [StealthSystemBaseClass]
    [Serializable]
    public class BasePurview<A>
        : IPurview<A>
        , IComposite
        where A : class, IAlert, new()
    {
        protected IAlertTypedPerception<A> _ParentPerception;
        public IAlertTypedPerception<A> ParentPerception
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

        IPerception IPurview.ParentPerception
        {
            get => ParentPerception;
            set => ParentPerception = value as IAlertTypedPerception<A>;
        }

        #region Constructors

        protected BasePurview()
        {
            _ParentPerception = null;
            Value = 0;
        }
        public BasePurview(IAlertTypedPerception<A> ParentPerception, int Value, string Attributes)
            : this()
        {
            this.ParentPerception = ParentPerception;
            this.Value = Value;
            this.Attributes = Attributes;
        }
        public BasePurview(BasePurview<A> Source)
            : this(Source.ParentPerception, Source.Value, Source.Attributes)
        {
        }

        #endregion
        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            string Attributes)
            => IPurview.WriteOptimized(Writer, Value, Attributes);

        public static void WriteOptimized(SerializationWriter Writer, BasePurview<A> Purview)
            => IPurview.WriteOptimized(Writer, Purview);

        public static void ReadOptimizedPurview(
            SerializationReader Reader,
            out int Value,
            out string Attributes)
            => IPurview.ReadOptimizedPurview(Reader, out Value, out Attributes);

        public static BasePurview<A> ReadOptimizedPurview(SerializationReader Reader, IAlertTypedPerception<A> ParentPerception)
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

        public override string ToString()
            => Value + "(E:" + EffectiveValue + ")" + "{" + Attributes + "}";

        public virtual IPurview<A> SetParentPerception(IAlertTypedPerception<A> ParentPerception)
        {
            _ParentPerception = ParentPerception;
            return this;
        }

        IPurview IPurview.SetParentPerception(IPerception ParentPerception)
            => SetParentPerception((IAlertTypedPerception<A>)ParentPerception);

        public virtual int GetEffectiveValue()
            => Value + GetPurviewAdjustment(ParentPerception, Value);

        public virtual List<string> GetPerviewAttributes()
            => Attributes?.CachedCommaExpansion();

        public virtual int GetPurviewAdjustment(IAlertTypedPerception<A> ParentPerception, int Value = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(ParentPerception.Owner, ParentPerception, Value);

        int IPurview.GetPurviewAdjustment(IPerception ParentPerception, int Value)
            => GetPurviewAdjustment(ParentPerception as IAlertTypedPerception<A>, Value);

        public BasePurview<A> SetValue(int Value)
        {
            this.Value = Value;
            return this;
        }

        public BasePurview<A> AdjustBy(int Amount)
            => SetValue(Value + Amount);

        #region Predicates

        public virtual bool HasAttribute(string Attribute)
            => GetPerviewAttributes().Contains(Attribute);

        public virtual bool HasAttributes(params string[] Attributes)
            => !Attributes.IsNullOrEmpty()
            && Attributes.All(a => HasAttribute(a));

        public virtual bool IsWithin(Cell Cell)
            => false;

        public virtual void ClearCaches()
        {
        }

        #endregion
        #region Equatable

        public virtual bool Equals(IPurview Other)
            => Utils.EitherNull(this, Other, out bool areEqual)
            ? areEqual
            : Value == Other.Value
                || EffectiveValue == Other.EffectiveValue;

        #endregion
        #region Comparable

        public virtual int CompareTo(IPurview Other)
            => ((IPurview)this).CompareTo(Other);

        #endregion
        #region Conversion

        public static explicit operator int(BasePurview<A> Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
