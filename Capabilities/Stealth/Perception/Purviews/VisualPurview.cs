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
    [Serializable]
    public class VisualPurview
        : BasePurview<Visual>
        , ILinePurview
        , IDiffusingPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => IDiffusingPurview.DefaultDiffuser;

        public override bool Occludes => true;

        private BaseDoubleDiffuser _Diffuser = DefaultDiffuser;
        public virtual BaseDoubleDiffuser Diffuser => _Diffuser;

        #region Constructors

        public VisualPurview()
            : base()
        {
        }
        public VisualPurview(
            IAlertTypedPerception<Visual> ParentPerception,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, IPurview.DEFAULT_VALUE)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(ParentPerception?.Name ?? "NO_PERCEPTION"),
                    Debug.Arg(nameof(Value), Value),
                });

            _Diffuser = Diffuser;
        }
        public VisualPurview(
            IAlertTypedPerception<Visual> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Value)
        {
            _Diffuser = Diffuser;
        }
        public VisualPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value)
        {
            _Diffuser = Diffuser;
        }
        public VisualPurview(VisualPurview Source)
            : base(Source)
        {
            _Diffuser = Source.Diffuser;
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
            _Diffuser = Reader.ReadComposite() as BaseDoubleDiffuser;
        }

        #endregion

        public override string ToString()
            => base.ToString();

        public override void Configure(Dictionary<string, object> args = null)
        {
            if (!args.IsNullOrEmpty())
            {
                if (args.ContainsKey(nameof(ConfigureDiffuser))
                    && args[nameof(ConfigureDiffuser)] is Dictionary<string, object> difuserArgs)
                {
                    ConfigureDiffuser(difuserArgs);
                }
                if (args.ContainsKey(nameof(Value))
                    && args[nameof(Value)] is int valueArg)
                {
                   SetValue(valueArg);
                }
            }
        }

        public virtual void ConfigureDiffuser(Dictionary<string, object> args = null)
            => ((IDiffusingPurview)this).ConfigureDiffuser(args);

        public override int GetPurviewAdjustment(IPerception ParentPerception, int Value = 0)
            => base.GetPurviewAdjustment(ParentPerception, Value);

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
