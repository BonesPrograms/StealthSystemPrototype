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
    public class BasePurview : IPurview, IComposite, IComparable<BasePurview>
    {
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        public static int MIN_VALUE => 0;
        public static int MAX_VALUE => 84;

        private static PurviewTypes[] _RadiusFlagValues;
        public static PurviewTypes[] RadiusFlagValues => _RadiusFlagValues ??= Enum.GetValues(typeof(PurviewTypes)) as PurviewTypes[] 
            ?? new PurviewTypes[0];

        private int Value;

        public string Attributes;
        private BaseDoubleDiffuser DiffusionSequence;

        private double[] _Diffusions;

        public int EffectiveValue => GetEffectiveValue();

        #region Constructors

        protected BasePurview()
        {
            Value = 0;
            Flags = PurviewTypes.None;
            DiffusionSequence = null;
            _Diffusions = null;
        }
        public BasePurview(int Value, PurviewTypes Flags, BaseDoubleDiffuser DiffusionSequence = null)
            : this()
        {
            this.Value = Value;
            this.Flags = Flags;
            this.DiffusionSequence = DiffusionSequence ?? DefaultDiffuser;
        }
        public BasePurview(int Value, BaseDoubleDiffuser DiffusionSequence = null)
            : this(Value, PurviewTypes.Line, DiffusionSequence)
        {
        }
        public BasePurview(BasePurview Source)
            : this(Source.Value, Source.Flags, Source.DiffusionSequence)
        {
        }
        public BasePurview(int Value, BasePurview Source)
            : this(Value, Source.Flags, Source.DiffusionSequence)
        {
        }
        public BasePurview(BasePurview Source, PurviewTypes Flags)
            : this(Source.Value, Flags, Source.DiffusionSequence)
        {
        }
        public BasePurview(BasePurview Source, BaseDoubleDiffuser DiffusionSequence = null)
            : this(Source.Value, Source.Flags, DiffusionSequence)
        {
        }

        #endregion

        public void Deconstruct(out int Radius, out PurviewTypes Flags)
        {
            Radius = GetValue();
            Flags = this.Flags;
        }

        private static string FlagString(PurviewTypes FlagValue)
            => RadiusFlagValues.Contains(FlagValue)
            ? FlagValue.ToString().Acronymize()
            : "?";

        protected static string FlagStrings(PurviewTypes Flags, string Accumulator, PurviewTypes FlagValue)
            => (FlagValue == PurviewTypes.None
                    && Flags != FlagValue)
                || !Flags.HasFlag(FlagValue)
            ? Accumulator
            : Accumulator + FlagString(FlagValue);

        protected string FlagStrings(string Accumulator, PurviewTypes FlagValue)
            => FlagStrings(Flags, Accumulator, FlagValue);

        public string FlagsString()
            => RadiusFlagValues
                ?.Aggregate("", FlagStrings)
            ?? "?";

        public override string ToString()
            => GetValue() + "(" + EffectiveValue + "/" + Value + ")" + 
            "{" + FlagsString() + "}";

        public int GetValue()
            => Value.Clamp(MIN_VALUE, MAX_VALUE);

        public int GetEffectiveValue()
            => Diffusions()
                ?.Where(d => d > 0)
                ?.Count()
            ?? 0;

        public InclusiveRange AsInclusiveRange()
            => new(GetValue());

        public BasePurview SetValue(int Value)
        {
            this.Value = Value;
            return this;
        }

        public BasePurview AdjustBy(int Amount)
            => SetValue(Value + Amount);

        #region Predicates

        public static bool IsLine(BasePurview Radius)
            => Radius.Flags.HasFlag(PurviewTypes.Line);

        public static bool IsPathing(BasePurview Radius)
            => Radius.Flags.HasFlag(PurviewTypes.Pathing);

        public static bool IsArea(BasePurview Radius)
            => Radius.Flags.HasFlag(PurviewTypes.Area);

        public static bool Occludes(BasePurview Radius)
            => Radius.Flags.HasFlag(PurviewTypes.Occludes);

        public static bool Diffuses(BasePurview Radius)
            => Radius.Flags.HasFlag(PurviewTypes.Diffuses);

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
            ? _Diffusions ??= DiffusionSequence[..]
            : _Diffusions ??= new double[GetValue()].Select(d => 1.0).ToArray();

        public double GetDiffusion(int Distance)
            => Diffusions()[Distance.Clamp(AsInclusiveRange())];

        public string GetDiffusionDebug(bool Inline = true)
            => DiffusionSequence.ToString(Short: false, Inline: Inline);

        #region Comparison

        public int CompareValueTo(BasePurview other)
            => Value - other.Value;

        public int CompareLineTo(BasePurview other)
            => Flags.CompareTo(other.Flags);

        public int CompareTo(BasePurview other)
        {
            int flagComp = 0;
            foreach (PurviewTypes flag in RadiusFlagValues)
                flagComp += Flags.HasFlag(flag).CompareTo(other.Flags.HasFlag(flag));

            int valueCOmp = Value - other.Value;

            return valueCOmp + flagComp;
        }

        #endregion
        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            PurviewTypes Flags,
            BaseDoubleDiffuser DiffusionSequence)
        {
            Writer.WriteOptimized(Value);
            Writer.WriteOptimized((int)Flags);
            DiffusionSequence.Write(Writer);
        }
        public static void WriteOptimized(SerializationWriter Writer, BasePurview Radius)
            => WriteOptimized(Writer, Radius.Value, Radius.Flags, Radius.DiffusionSequence);

        public static void ReadOptimizedRadius(
            SerializationReader Reader,
            out int Value,
            out PurviewTypes Flags,
            out BaseDoubleDiffuser DiffusionSequence)
        {
            Value = Reader.ReadOptimizedInt32();
            Flags = (PurviewTypes)Reader.ReadOptimizedInt32();
            DiffusionSequence = Reader.ReadComposite() as BaseDoubleDiffuser;
        }
        public static BasePurview ReadOptimizedRadius(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out int value, out PurviewTypes flags, out BaseDoubleDiffuser DiffusionSequence);
            return new(value, flags, DiffusionSequence);
        }

        public void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer, Value, Flags, DiffusionSequence);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out Value, out Flags, out DiffusionSequence);
        }

        #endregion
        #region Conversion

        public static explicit operator int(BasePurview Operand)
            => Operand.GetValue();

        #endregion
    }
}
