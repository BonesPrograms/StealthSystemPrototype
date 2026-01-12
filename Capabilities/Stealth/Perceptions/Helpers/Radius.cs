using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public struct Radius : IComposite, IComparable<Radius>
    {
        public static Radius Empty => new(0, 0..0, 0);

        [Flags]
        [Serializable]
        public enum RadiusFlags : int
        {
            Line,
            Pathing,
            Area,
            Occludes,
            Tapers,
        }

        private int Value;
        private Range Clamp;
        public RadiusFlags Flags;

        public Radius(int Value, Range Clamp, RadiusFlags Flags)
        {
            this.Value = Value;
            this.Clamp = Clamp;
            this.Flags = Flags;
        }
        public Radius(int Value, Range Clamp)
            : this(Value, Clamp, 0)
        {
        }
        public Radius(int Value)
            : this(Value, BasePerception.RADIUS_CLAMP, 0)
        {
        }
        public Radius(Radius Source)
            : this(Source.Value, Source.Clamp, Source.Flags)
        {
        }
        public Radius(Radius Source, Range Clamp)
            : this(Source.Value, Clamp, Source.Flags)
        {
        }
        public Radius(int Value, Radius Source)
            : this(Value, Source.Clamp, Source.Flags)
        {
        }
        public Radius(Radius Source, RadiusFlags Flags)
            : this(Source.Value, Source.Clamp, Source.Flags)
        {
        }

        public readonly void Deconstruct(out int Radius, out RadiusFlags Flags)
        {
            Radius = GetValue();
            Flags = this.Flags;
        }

        public readonly int GetValue()
            => Value.Clamp(Clamp);

        public readonly int GetValueCap(int? Cap = null)
            => Value.ClampCap(Clamp, Cap);

        public readonly bool IsLine()
            => Flags.HasFlag(RadiusFlags.Line);

        public readonly bool IsPathing()
            => Flags.HasFlag(RadiusFlags.Pathing);

        public readonly bool IsArea()
            => Flags.HasFlag(RadiusFlags.Area);

        public readonly bool Occludes()
            => Flags.HasFlag(RadiusFlags.Occludes);

        public readonly bool Tapers()
            => Flags.HasFlag(RadiusFlags.Tapers);

        public readonly Radius AdjustBy(int Amount)
            => new(Amount, this);

        #region Comparison

        public readonly int CompareValueTo(Radius other)
            => Value - other.Value;

        private static int CompareBools(bool x, bool y)
        {
            if (x == y)
                return 0;
            if (x && !y)
                return 1;
            if (!x && y)
                return -1;
            return 0;
        }
        public readonly int CompareLineTo(Radius other)
            => Flags.CompareTo(other.Flags);

        public int CompareTo(Radius other)
        {
            int flagComp = 0;
            foreach (RadiusFlags flag in Enum.GetValues(typeof(RadiusFlags)) ?? new RadiusFlags[0])
                flagComp += CompareBools(Flags.HasFlag(flag), other.Flags.HasFlag(flag));

            int valueCOmp = Value - other.Value;

            return valueCOmp + flagComp;
        }

        #endregion
        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            int Value,
            Range Clamp,
            RadiusFlags Flags)
        {
            Writer.WriteOptimized(Value);
            Writer.WriteOptimized(Clamp);
            Writer.WriteOptimized((int)Flags);
        }
        public static void WriteOptimized(SerializationWriter Writer, Radius Radius)
            => WriteOptimized(Writer, Radius.Value, Radius.Clamp, Radius.Flags);

        public static void ReadOptimizedRadius(
            SerializationReader Reader,
            out int Value,
            out Range Clamp,
            out RadiusFlags Flags)
        {
            Value = Reader.ReadOptimizedInt32();
            Clamp = Reader.ReadOptimizedRange();
            Flags = (RadiusFlags)Reader.ReadOptimizedInt32();
        }

        public static Radius ReadOptimizedRadius(SerializationReader Reader)
        {
            ReadOptimizedRadius(Reader, out int value, out Range clamp, out RadiusFlags flags);
            return new(value, clamp, flags);
        }

        public readonly void Write(SerializationWriter Writer)
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
