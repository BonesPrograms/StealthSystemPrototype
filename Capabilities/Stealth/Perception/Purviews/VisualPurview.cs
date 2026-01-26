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
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [Serializable]
    public class VisualPurview
        : BasePurview<Visual>
        , ILinePurview
        , IDiffusingPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => IDiffusingPurview.DefaultDiffuser;

        public override bool Occludes => true;

        private BaseDoubleDiffuser _Diffuser;
        public virtual BaseDoubleDiffuser Diffuser
        {
            get => _Diffuser;
            set => _Diffuser = value;
        }

        #region Constructors

        public VisualPurview()
            : base()
        {
        }
        public VisualPurview(
            IAlertTypedPerception<Visual, IPurview<Visual>> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Value)
        {
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public VisualPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value, Diffuser)
        {
        }
        public VisualPurview(VisualPurview Source)
            : this(Source.ParentPerception, Source.Value, Source.Diffuser)
        {
        }
        public VisualPurview(SerializationReader Reader, IAlertTypedPerception<Visual, VisualPurview> ParentPerception)
            : base(Reader, ParentPerception as IAlertTypedPerception<Visual, IPurview<Visual>>)
        {
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.Write(Diffuser);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Diffuser = Reader.ReadComposite() as BaseDoubleDiffuser;
        }

        #endregion

        public override void Construct()
        {
            base.Construct();
            Diffuser ??= GetDefaultDiffuser();
        }

        public override string ToString()
            => base.ToString();

        public override int GetEffectiveValue()
            => base.GetEffectiveValue();

        public override int GetPurviewAdjustment(IAlertTypedPerception<Visual, IPurview<Visual>> ParentPerception, int Value = 0)
            => base.GetPurviewAdjustment(ParentPerception, Value);

        public void AssignDefaultDiffuser()
            => Diffuser = GetDefaultDiffuser();

        public virtual BaseDoubleDiffuser GetDefaultDiffuser()
            => DefaultDiffuser.SetSteps(Value) as BaseDoubleDiffuser;

        public virtual double Diffuse(int Value)
        {
            if (Diffuser == null)
                return Value;

            if (Diffuser.TryGetValue(Value, out double diffusedValue))
                return diffusedValue;

            return 0;
        }

        #region Predicates

        public override bool IsWithin(AlertContext Context)
            => base.IsWithin(Context);

        public override void ClearCaches()
        {
        }

        #endregion
        #region Equatable

        public override bool Equals(IPurview Other)
            => base.Equals(Other);

        #endregion
        #region Comparable

        public override int CompareTo(IPurview Other)
            => base.CompareTo(Other);

        #endregion
        #region Conversion

        public static explicit operator int(VisualPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
