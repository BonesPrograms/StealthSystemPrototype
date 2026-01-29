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
        protected IAlertTypedPerception<A, IPurview<A>> _ParentPerception;
        public virtual IAlertTypedPerception<A, IPurview<A>> ParentPerception
        {
            get => _ParentPerception;
            set => _ParentPerception = value;
        }
        IPerception IPurview.ParentPerception => ParentPerception;

        private int _Value;
        public int Value
        {
            get => _Value;
            protected set => _Value = value;
        }

        public int EffectiveValue => GetEffectiveValue();

        public abstract bool Occludes { get; }

        #region Constructors

        public BasePurview()
        {
            _ParentPerception = null;
            Value = 0;
            Construct();
        }
        protected BasePurview(IAlertTypedPerception<A, IPurview<A>> ParentPerception, int Value)
            : this()
        {
            this.ParentPerception = ParentPerception;
            this.Value = Value;
        }
        public BasePurview(BasePurview<A> Source)
            : this(Source.ParentPerception, Source.Value)
        {
        }
        public BasePurview(SerializationReader Reader, IAlertTypedPerception<A, IPurview<A>> ParentPerception)
        {
            _ParentPerception = ParentPerception;
            Read(Reader);
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
            ParentPerception = Reader.ReadComposite() as IAlertTypedPerception<A, IPurview<A>>;
            Value = Reader.ReadOptimizedInt32();
        }

        #endregion

        public override string ToString()
            => Value + "(E:" + EffectiveValue + ")" + (Occludes ? "[" + nameof(Occludes) + "]" : null);

        public virtual void Construct()
        {
        }

        public virtual IPurview<A> SetParentPerception(IAlertTypedPerception<A, IPurview<A>> ParentPerception)
        {
            _ParentPerception = ParentPerception;
            return this;
        }

        IPurview IPurview.SetParentPerception(IPerception ParentPerception)
            => SetParentPerception((IAlertTypedPerception<A, IPurview<A>>)ParentPerception);

        public virtual int GetEffectiveValue()
            => Value + GetPurviewAdjustment(ParentPerception, Value);

        public virtual int GetPurviewAdjustment(IAlertTypedPerception<A, IPurview<A>> ParentPerception, int Value = 0)
            => AdjustTotalPerceptionLevelEvent.GetFor(ParentPerception.Owner, ParentPerception, Value);

        int IPurview.GetPurviewAdjustment(IPerception ParentPerception, int Value)
            => GetPurviewAdjustment(ParentPerception as IAlertTypedPerception<A, IPurview<A>>, Value);

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
