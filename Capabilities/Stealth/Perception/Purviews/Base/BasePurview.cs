using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [StealthSystemBaseClass]
    [Serializable]
    public class BasePurview
        : IPurview
        , IComparable<BasePurview>
    {
        protected BasePerception _ParentPerception;
        public BasePerception ParentPerception
        {
            get => _ParentPerception;
            set
            {
                using Indent indent = new(1);
                Debug.LogCaller(indent,
                    ArgPairs: new Debug.ArgPair[]
                    {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(value?.Name ?? "NO_PERCEPTION"),
                    Debug.Arg(nameof(Value), Value),
                    });

                SetParentPerception(value);
            }
        }

        protected int _Value;
        public int Value
        {
            get => _Value;
            protected set => _Value = value;
        }

        private int? _EffectiveValue;
        public int EffectiveValue
        {
            get
            {
                if (_EffectiveValue == null
                    && !GettingValueAdjustment)
                {
                    GettingValueAdjustment.Toggle();

                    _EffectiveValue = Value + GetPurviewValueAdjustment(ParentPerception, Value);

                    GettingValueAdjustment.Toggle();
                }
                return _EffectiveValue ?? Value;
            }
        }
        private bool GettingValueAdjustment = false;

        public virtual bool Occludes { get; } = false;

        public virtual Type AlertType { get; }

        #region Constructors

        public BasePurview()
        {
            ParentPerception = null;
            Value = 0;
            _EffectiveValue = null;
        }
        protected BasePurview(BasePerception ParentPerception)
            : this()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentPerception?.Name ?? "NO_PERCEPTION"),
                });

            this.ParentPerception = ParentPerception;
        }
        protected BasePurview(BasePerception ParentPerception, int Value)
            : this(ParentPerception)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentPerception?.Name ?? "NO_PERCEPTION"),
                    Debug.Arg(nameof(Value), Value),
                });

            this.Value = Value;
        }
        public BasePurview(BasePurview Source)
            : this(Source.ParentPerception, Source.Value)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(Source)),
                });
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteComposite(ParentPerception);
            Writer.WriteOptimized(Value);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ParentPerception = Reader.ReadComposite() as BasePerception;
            Value = Reader.ReadOptimizedInt32();
        }

        #endregion

        public IPerception GetParentPerception()
            => ParentPerception;

        public virtual Type GetAlertType()
            => AlertType;

        public int GetValue()
            => Value;

        public int GetEffectiveValue()
            => EffectiveValue;

        public bool GetOccludes()
            => Occludes;

        public override string ToString()
            => Value + "(E:" + EffectiveValue + ")" +
            (Occludes ? "[" + nameof(Occludes) + "]" : null) +
            "<" +(GetAlertType() != null ?  GetAlertType().ToStringWithGenerics() : "NO_ALERT") + ">";

        public virtual void Configure(Dictionary<string, object> args = null)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(args), args?.Count ?? 0),
                });

            if (!args.IsNullOrEmpty())
            {
                args.ForEach(kvp => Debug.Log(kvp.Key, kvp.Value, Indent: indent[1]));

                if (args.ContainsKey(nameof(ParentPerception))
                    && args[nameof(ParentPerception)] is BasePerception parentPerceptionArg)
                {
                    ParentPerception = parentPerceptionArg;
                }
                if (args.ContainsKey(nameof(Value))
                    && args[nameof(Value)] is int valueArg)
                {
                    Value = valueArg;
                }
            }
        }

        public BasePurview SetParentPerception(BasePerception ParentPerception)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentPerception?.Name ?? "NO_PERCEPTION"),
                });

            if (ParentPerception != null
                && !ParentPerception.IsCompatibleWith(this))
                throw new ArgumentException(
                    message: GetType().ToStringWithGenerics() + " requires a " + nameof(ParentPerception) +
                        " compatible with " + nameof(IAlert) + " of type " + AlertType.ToStringWithGenerics() + ". " +
                        ParentPerception.GetName() + " uses " + ParentPerception.GetAlertType().ToStringWithGenerics(),
                    paramName: nameof(ParentPerception));

            _ParentPerception = ParentPerception;
            return this;
        }

        IPurview IPurview.SetParentPerception(IPerception ParentPerception)
            => SetParentPerception(ParentPerception as BasePerception);

        public virtual int GetPurviewValueAdjustment(BasePerception ParentPerception, int Value = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(ParentPerception.GetOwner(), ParentPerception, Value);

        int IPurview.GetPurviewValueAdjustment(IPerception ParentPerception, int Value)
            => GetPurviewValueAdjustment(ParentPerception as BasePerception, Value);

        public void SetValue(int Value)
            => this.Value = Value;

        public void MaybeSetValue(int? Value)
        {
            if (Value != null)
                SetValue((int)Value);
        }

        public void AdjustBy(int Amount)
            => SetValue(Value + Amount);

        #region Predicates

        public virtual bool IsForAlert(IAlert Alert)
            => Alert.IsType(GetAlertType());

        public virtual bool IsWithin(AlertContext Context)
            => false;

        public virtual void ClearCaches()
        {
            _EffectiveValue = null;
        }

        #endregion
        #region Equatable

        public virtual bool Equals(IPurview Other)
            => EitherNull(this, Other, out bool areEqual)
            ? areEqual
            : GetValue() == Other.GetValue()
                || GetEffectiveValue() == Other.GetEffectiveValue();

        #endregion
        #region Comparable

        public int CompareValueTo(IPurview Other)
            => GetValue() - Other.GetValue();

        public int CompareEffectiveValueTo(IPurview Other)
            => GetEffectiveValue() - Other.GetEffectiveValue();

        public virtual int CompareTo(IPurview Other)
            => EitherNull(this, Other, out int comparison)
            ? comparison
            : CompareValueTo(Other) + CompareEffectiveValueTo(Other);

        public virtual int CompareTo(BasePurview Other)
            => CompareTo(Other as IPurview);

        #endregion
        #region Conversion

        public static explicit operator int(BasePurview Operand)
            => Operand.GetEffectiveValue();

        #endregion
    }
}
