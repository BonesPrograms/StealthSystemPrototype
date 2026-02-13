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

        public Cell Origin => throw new NotImplementedException();

        #region Constructors

        public VisualPurview()
            : base()
        {
        }
        public VisualPurview(
            IAlertTypedPerception<Visual> ParentPerception,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception as BasePerception, IPurview.DEFAULT_VALUE)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(BaseValue), BaseValue),
                });

            _Diffuser = Diffuser ?? _Diffuser;
        }
        public VisualPurview(
            IAlertTypedPerception<Visual> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception as BasePerception, Value)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public VisualPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public VisualPurview(VisualPurview Source)
            : base(Source)
        {
            _Diffuser = Diffuser ?? _Diffuser;
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
                if (args.ContainsKey(nameof(BaseValue))
                    && args[nameof(BaseValue)] is int valueArg)
                {
                   SetValue(valueArg);
                }
            }
        }

        public virtual void ConfigureDiffuser(Dictionary<string, object> args = null)
        {
            if (!args.IsNullOrEmpty())
            {
                if (args.ContainsKey(nameof(Diffuser.SetSteps))
                    && args[nameof(Diffuser.SetSteps)] is int valueArg)
                {
                    Diffuser.SetSteps(valueArg);
                }
            }
        }

        public virtual double Diffuse(int Value)
        {
            if (Diffuser == null)
                return Value;

            if (EffectiveValue >= Diffuser.Count)
                Diffuser.SetSteps(EffectiveValue);

            if (Diffuser.TryGetValue(EffectiveValue, out double diffusionFactor))
                return Value * diffusionFactor;

            return 0;
        }

        public override int GetEffectiveLevel(AlertContext Context)
        {
            if (ParentPerception == null
                || !CheckWithinLine(Context, out int distance))
                return 0;

            return (int)Diffuse(distance);
        }

        #region Predicates

        public virtual bool CheckWithinLine(Cell PerceiverLocation, Cell AlertLocation, out int Distance)
        {
            Distance = -1;
            if (PerceiverLocation is not Cell { InActiveZone: true } origin
                || AlertLocation is not Cell { InActiveZone: true } destination)
                return false;

            Distance = origin.CosmeticDistanceToCell(destination);

            return Distance <= GetEffectiveValue()
                && (!GetOccludes()
                    || origin.HasLOSTo(destination));
        }

        public virtual bool CheckWithinLine(AlertContext Context, out int Distance)
            => CheckWithinLine(
                PerceiverLocation: Context.Perceiver.CurrentCell,
                AlertLocation: Context.AlertLocation,
                Distance: out Distance);

        #endregion

        public override void ClearCaches()
        {
        }

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
