using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

using static StealthSystemPrototype.Capabilities.Stealth.DelayedLinearDoubleDiffuser;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Radius : IComposite, IComparable<Radius>
    {
        public static Radius Empty => new(0, 0..0, 0);

        public static BaseDoubleDiffuser DefaultDiffuser = new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        [Flags]
        [Serializable]
        public enum RadiusFlags : int
        {
            None = 0,
            Line = 1,
            Area = 2,
            Pathing = 4,
            Occludes = 8,
            Diffuses = 16,
        }

        private static RadiusFlags[] _RadiusFlagValues;
        public static RadiusFlags[] RadiusFlagValues => _RadiusFlagValues ??= Enum.GetValues(typeof(RadiusFlags)) as RadiusFlags[] 
            ?? new RadiusFlags[0];

        private int Value;
        private InclusiveRange Clamp;
        [NonSerialized]
        public RadiusFlags Flags;
        private BaseDoubleDiffuser DiffusionSequence;

        public int EffectiveValue => GetEffectiveValue();

        #region Constructors

        protected Radius()
        {
            Value = 0;
            Clamp = default;
            Flags = RadiusFlags.None;
            DiffusionSequence = null;
        }
        public Radius(int Value, InclusiveRange Clamp, RadiusFlags Flags, BaseDoubleDiffuser DiffusionSequence = null)
            : base()
        {
            this.Value = Value;
            this.Clamp = Clamp;
            this.Flags = Flags;
            this.DiffusionSequence = DiffusionSequence ?? DefaultDiffuser;
        }
        public Radius(int Value, InclusiveRange Clamp, BaseDoubleDiffuser DiffusionSequence = null)
            : this(Value, Clamp, RadiusFlags.Line, DiffusionSequence)
        {
        }
        public Radius(int Value, BaseDoubleDiffuser DiffusionSequence = null)
            : this(Value, BasePerception.RADIUS_CLAMP, RadiusFlags.Line, DiffusionSequence)
        {
        }
        public Radius(Radius Source)
            : this(Source.Value, Source.Clamp, Source.Flags, Source.DiffusionSequence)
        {
        }
        public Radius(Radius Source, InclusiveRange Clamp)
            : this(Source.Value, Clamp, Source.Flags, Source.DiffusionSequence)
        {
        }
        public Radius(int Value, Radius Source)
            : this(Value, Source.Clamp, Source.Flags, Source.DiffusionSequence)
        {
        }
        public Radius(Radius Source, RadiusFlags Flags)
            : this(Source.Value, Source.Clamp, Flags, Source.DiffusionSequence)
        {
        }
        public Radius(Radius Source, BaseDoubleDiffuser DiffusionSequence = null)
            : this(Source.Value, Source.Clamp, Source.Flags, DiffusionSequence)
        {
        }

        #endregion

        public void Deconstruct(out int Radius, out RadiusFlags Flags)
        {
            Radius = GetValue();
            Flags = this.Flags;
        }

        private static string FlagString(RadiusFlags FlagValue)
            => RadiusFlagValues.Contains(FlagValue)
            ? FlagValue.ToString().Acronymize()
            : "?";

        protected static string FlagStrings(RadiusFlags Flags, string Accumulator, RadiusFlags FlagValue)
            => (FlagValue == RadiusFlags.None
                    && Flags != FlagValue)
                || !Flags.HasFlag(FlagValue)
            ? Accumulator
            : Accumulator + FlagString(FlagValue);

        protected string FlagStrings(string Accumulator, RadiusFlags FlagValue)
            => FlagStrings(Flags, Accumulator, FlagValue);

        public string FlagsString()
            => RadiusFlagValues
                ?.Aggregate("", FlagStrings)
            ?? "?";

        public override string ToString()
            => GetValue() + "(" + EffectiveValue + ")" + "/" + Value + 
            "{" + FlagsString() + "}";

        public int GetValue()
            => Value.Clamp(Clamp);

        public int GetEffectiveValue()
            => Diffusions()
                ?.Where(d => d > 0)
                ?.Count()
            ?? 0;

        public InclusiveRange AsInclusiveRange()
            => new(GetValue());

        public Radius SetValue(int Value)
        {
            this.Value = Value;
            return this;
        }
        public Radius SetClamp(InclusiveRange Clamp)
        {
            this.Clamp = Clamp;
            return this;
        }

        public Radius AdjustBy(int Amount)
            => SetValue(Value + Amount);

        public Radius AdjustClampBy(int Amount)
            => SetClamp(Clamp.AdjustBy(Amount).Clamp(Clamp));

        public Radius AdjustClampBy(InclusiveRange OtherRange)
            => SetClamp(Clamp.AdjustBy(OtherRange).Clamp(Clamp));

        #region Predicates

        public static bool IsLine(Radius Radius)
            => Radius.Flags.HasFlag(RadiusFlags.Line);

        public static bool IsPathing(Radius Radius)
            => Radius.Flags.HasFlag(RadiusFlags.Pathing);

        public static bool IsArea(Radius Radius)
            => Radius.Flags.HasFlag(RadiusFlags.Area);

        public static bool Occludes(Radius Radius)
            => Radius.Flags.HasFlag(RadiusFlags.Occludes);

        public static bool Diffuses(Radius Radius)
            => Radius.Flags.HasFlag(RadiusFlags.Diffuses);

        public bool IsLine()
            => IsLine(this);

        public bool IsPathing()
            => IsPathing(this);

        public bool IsArea()
            => IsArea(this);

        public bool Occludes()
            => Occludes(this);

        public bool Diffuses()
            => Diffuses(this);

        #endregion

        public double[] Diffusions()
            => Diffuses()
                && DiffusionSequence?.SetSteps(GetValue()) != null
            ? DiffusionSequence[..]
            : new double[GetValue()].Select(d => 1.0).ToArray();

        public double GetDiffusion(int Distance)
            => Diffusions()[Distance.Clamp(AsInclusiveRange())];

        public string GetDiffusionDebug()
            => DiffusionSequence.ToString(false);

        #region Comparison

        public int CompareValueTo(Radius other)
            => Value - other.Value;

        public int CompareLineTo(Radius other)
            => Flags.CompareTo(other.Flags);

        public int CompareTo(Radius other)
        {
            int flagComp = 0;
            foreach (RadiusFlags flag in RadiusFlagValues)
                flagComp += Flags.HasFlag(flag).CompareTo(other.Flags.HasFlag(flag));

            int valueCOmp = Value - other.Value;

            return valueCOmp + flagComp;
        }

        #endregion
        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            InclusiveRange Clamp,
            RadiusFlags Flags,
            BaseDoubleDiffuser DiffusionSequence)
        {
            Writer.WriteOptimized(Value);
            Clamp.WriteOptimized(Writer);
            Writer.WriteOptimized((int)Flags);
            DiffusionSequence.Write(Writer);
        }
        public static void WriteOptimized(SerializationWriter Writer, Radius Radius)
            => WriteOptimized(Writer, Radius.Value, Radius.Clamp, Radius.Flags, Radius.DiffusionSequence);

        public static void ReadOptimizedRadius(
            SerializationReader Reader,
            out int Value,
            out InclusiveRange Clamp,
            out RadiusFlags Flags,
            out BaseDoubleDiffuser DiffusionSequence)
        {
            Value = Reader.ReadOptimizedInt32();
            Clamp = Reader.ReadOptimizedRange();
            Flags = (RadiusFlags)Reader.ReadOptimizedInt32();
            DiffusionSequence = Reader.ReadComposite() as BaseDoubleDiffuser;
        }
        public static Radius ReadOptimizedRadius(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out int value, out InclusiveRange clamp, out RadiusFlags flags, out BaseDoubleDiffuser DiffusionSequence);
            return new(value, clamp, flags, DiffusionSequence);
        }

        public void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer, Value, Clamp, Flags, DiffusionSequence);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out Value, out Clamp, out Flags, out DiffusionSequence);
        }

        #endregion
        #region Conversion

        public static explicit operator int(Radius Operand)
            => Operand.GetValue();

        #endregion
    }
}
