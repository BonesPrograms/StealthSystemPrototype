using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class Radius : IComposite, IComparable<Radius>
    {
        public static Radius Empty => new(0, 0..0, 0);

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
        public RadiusFlags Flags;
        private DoubleDiffuser DiffusionSequence;

        #region Constructors

        public Radius(int Value, InclusiveRange Clamp, RadiusFlags Flags, DoubleDiffuser DiffusionSequence = null)
        {
            this.Value = Value;
            this.Clamp = Clamp;
            this.Flags = Flags;
            this.DiffusionSequence = DiffusionSequence;
        }
        public Radius(int Value, InclusiveRange Clamp, DoubleDiffuser DiffusionSequence = null)
            : this(Value, Clamp, RadiusFlags.Line, DiffusionSequence)
        {
        }
        public Radius(int Value, DoubleDiffuser DiffusionSequence = null)
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
        public Radius(Radius Source, DoubleDiffuser DiffusionSequence = null)
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
            => GetValue().ToString() + "(" +
            FlagsString() +
            ")";

        public int GetValue()
            => Value.Clamp(Clamp);

        public bool IsLine()
            => Flags.HasFlag(RadiusFlags.Line);

        public bool IsPathing()
            => Flags.HasFlag(RadiusFlags.Pathing);

        public bool IsArea()
            => Flags.HasFlag(RadiusFlags.Area);

        public bool Occludes()
            => Flags.HasFlag(RadiusFlags.Occludes);

        public bool Diffuses()
            => Flags.HasFlag(RadiusFlags.Diffuses);

        public Radius AdjustBy(int Amount)
            => new(Amount, this);

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

        public double[] Diffusions()
            => Diffuses()
                && DiffusionSequence?.SetSteps(GetValue()) != null
            ? DiffusionSequence[..]
            : new double[GetValue() - 1].Select(d => 1.0).ToArray();

        public double GetDiffusion(int Distance)
            => Diffusions()[Distance.Clamp(0, GetValue() - 1)];

        #endregion
        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            InclusiveRange Clamp,
            RadiusFlags Flags)
        {
            Writer.WriteOptimized(Value);
            Clamp.WriteOptimized(Writer);
            Writer.WriteOptimized((int)Flags);
        }
        public static void WriteOptimized(SerializationWriter Writer, Radius Radius)
            => WriteOptimized(Writer, Radius.Value, Radius.Clamp, Radius.Flags);

        public static void ReadOptimizedRadius(
            SerializationReader Reader,
            out int Value,
            out InclusiveRange Clamp,
            out RadiusFlags Flags)
        {
            Value = Reader.ReadOptimizedInt32();
            Clamp = Reader.ReadOptimizedRange();
            Flags = (RadiusFlags)Reader.ReadOptimizedInt32();
        }
        public static Radius ReadOptimizedRadius(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out int value, out InclusiveRange clamp, out RadiusFlags flags);
            return new(value, clamp, flags);
        }

        public void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer, Value, Clamp, Flags);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out Value, out Clamp, out Flags);
        }

        #endregion
        #region Conversion

        public static explicit operator int(Radius Operand)
            => Operand.GetValue();

        #endregion
    }
}
