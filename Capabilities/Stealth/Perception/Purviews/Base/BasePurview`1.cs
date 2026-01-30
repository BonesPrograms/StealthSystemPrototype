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

using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [StealthSystemBaseClass]
    [Serializable]
    public abstract class BasePurview<A>
        : IPurview<A>
        , IComposite
        where A : IAlert, new()
    {
        private IPerception _ParentPerception;
        public IPerception ParentPerception
        {
            get => _ParentPerception;
            set => SetParentPerception(value);
        }

        private int _Value;
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

                    _EffectiveValue = Value + GetPurviewAdjustment(ParentPerception, Value);

                    GettingValueAdjustment.Toggle();
                }
                return _EffectiveValue ?? Value;
            }
        }
        private bool GettingValueAdjustment = false;

        public abstract bool Occludes { get; }

        #region Constructors

        public BasePurview()
        {
            this.ParentPerception = null;
            Value = 0;
            _EffectiveValue = null;
        }
        protected BasePurview(IPerception ParentPerception)
            : this()
        {
            this.ParentPerception = ParentPerception;
        }
        protected BasePurview(IPerception ParentPerception, int Value)
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
        public BasePurview(BasePurview<A> Source)
            : this(Source.ParentPerception, Source.Value)
        {
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.Write(ParentPerception);
            Writer.WriteOptimized(Value);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ParentPerception = Reader.ReadComposite() as IPerception;
            Value = Reader.ReadOptimizedInt32();
        }

        #endregion

        public override string ToString()
            => Value + "(E:" + EffectiveValue + ")" + (Occludes ? "[" + nameof(Occludes) + "]" : null);

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
                    && args[nameof(ParentPerception)] is IPerception parentPerceptionArg)
                {
                    _ParentPerception = parentPerceptionArg;
                }
                if (args.ContainsKey(nameof(Value))
                    && args[nameof(Value)] is int valueArg)
                {
                    Value = valueArg;
                }
            }
        }

        public IPurview SetParentPerception(IPerception ParentPerception)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentPerception),
                });

            if (!ParentPerception.IsCompatibleWith(this))
                throw new ArgumentException(
                    message: GetType().ToStringWithGenerics() + " requires a " + nameof(ParentPerception) + 
                        " compatible with " + nameof(IAlert) + " of type " + typeof(A) + ". " +
                        ParentPerception.Name + " uses " + ParentPerception.AlertType.ToStringWithGenerics(), 
                    paramName: nameof(ParentPerception));

            _ParentPerception = ParentPerception;
            return this;
        }

        public virtual bool IsForAlert(IAlert Alert)
            => Alert.IsType(typeof(A));

        public virtual int GetPurviewAdjustment(IPerception ParentPerception, int Value = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(ParentPerception.Owner, ParentPerception, Value);

        public BasePurview<A> SetValue(int Value)
        {
            this.Value = Value;
            return this;
        }

        public BasePurview<A> AdjustBy(int Amount)
            => SetValue(Value + Amount);

        #region Predicates

        public virtual bool IsWithin(AlertContext Context)
            => false;

        public virtual void ClearCaches()
        {
            _EffectiveValue = null;
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
