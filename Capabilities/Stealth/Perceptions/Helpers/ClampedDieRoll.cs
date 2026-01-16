using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Logging;

using XRL.Rules;
using XRL.World;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    [Serializable]
    public class ClampedDieRoll : IComposite
    {
        private DieRoll DieRoll;
        private InclusiveRange Clamp;

        private ClampedDieRoll()
        {
            DieRoll = default;
            Clamp = default;
        }
        public ClampedDieRoll(DieRoll DieRoll, InclusiveRange Clamp)
            : this()
        {
            this.DieRoll = DieRoll.Clamp(Clamp);
            this.Clamp = Clamp;
        }
        public ClampedDieRoll(DieRoll DieRoll)
            : this(DieRoll, InclusiveRange.GetFromDieRoll(DieRoll))
        {
        }
        public ClampedDieRoll(InclusiveRange InclusiveRange)
            : this(InclusiveRange.ToDieRoll(), InclusiveRange)
        {
        }
        public ClampedDieRoll(InclusiveRange InclusiveRange, InclusiveRange Clamp)
            : this(InclusiveRange.ToDieRoll(), Clamp)
        {
        }
        public ClampedDieRoll(ClampedDieRoll DieRoll, InclusiveRange Clamp)
            : this(DieRoll.DieRoll, Clamp)
        {
        }
        public ClampedDieRoll(DieRoll DieRoll, ClampedDieRoll Clamp)
            : this(DieRoll, Clamp.Clamp)
        {
        }

        public void Deconstruct(out DieRoll DieRoll)
        {
            DieRoll = GetDieRoll();
        }

        public override string ToString()
            => GetDieRoll().ToString();

        public static DieRoll GetDieRoll(DieRoll DieRoll, InclusiveRange Clamp)
            => DieRoll.Clamp(Clamp)
            ?? Clamp.ToDieRoll();

        public DieRoll GetDieRoll()
            => GetDieRoll(DieRoll, Clamp);

        public ClampedDieRoll SetDieRoll(DieRoll DieRoll)
        {
            this.DieRoll = DieRoll;
            return this;
        }
        public ClampedDieRoll SetClamp(InclusiveRange Clamp)
        {
            this.Clamp = Clamp;
            return this;
        }

        public ClampedDieRoll AdjustBy(int Amount)
        {
            if (DieRoll.TryAdjustBy(Amount, out DieRoll dieRoll))
                SetDieRoll(dieRoll.Clamp(Clamp));
            else
                DieRoll.AdjustResult(Amount);
            return this;
        }

        public ClampedDieRoll AdjustClampBy(int Amount)
            => SetClamp(Clamp.AdjustBy(Amount).Clamp(Clamp));

        public ClampedDieRoll AdjustClampBy(InclusiveRange OtherRange)
            => SetClamp(Clamp.AdjustByValues(OtherRange).Clamp(Clamp));

        public int Average()
            => (int)GetDieRoll().Average();

        public int Breadth()
            => GetDieRoll().Max() - GetDieRoll().Min();

        public int Floor()
            => GetDieRoll().Min();

        public int Ceiling()
            => GetDieRoll().Max();

        public int Roll()
            => GetDieRoll().Resolve();

        public int RollSeeded(string Seed)
        {
            if (GetDieRoll() is not DieRoll dieRoll)
                return default;

            string channel = dieRoll.Channel;
            dieRoll.Channel = Seed;
            int roll = Roll();
            dieRoll.Channel = channel;
            return roll;
        }

        #region Serialization

        public static void WriteOptimized(
            SerializationWriter Writer,
            DieRoll DieRoll,
            InclusiveRange Clamp)
        {
            Writer.WriteOptimized(DieRoll.ToString());
            Clamp.WriteOptimized(Writer);
        }

        public static void WriteOptimized(
            SerializationWriter Writer,
            ClampedDieRoll ClampedRange)
            => WriteOptimized(Writer, ClampedRange.DieRoll, ClampedRange.Clamp);

        public static void ReadOptimizedClampedRange(
            SerializationReader Reader,
            out DieRoll DieRoll,
            out InclusiveRange Clamp)
        {
            DieRoll = new DieRoll(Reader.ReadOptimizedString());
            Clamp = InclusiveRange.ReadOptimizedInclusiveRange(Reader);
        }
        public static ClampedDieRoll ReadOptimizedClampedRange(SerializationReader Reader)
        {
            ReadOptimizedClampedRange(Reader, out DieRoll DieRoll, out InclusiveRange clamp);
            return new(DieRoll, clamp);
        }

        public void Write(SerializationWriter Writer)
        {
            WriteOptimized(Writer, DieRoll, Clamp);
        }
        public void Read(SerializationReader Reader)
        {
            ReadOptimizedClampedRange(Reader, out DieRoll, out Clamp);
        }

        #endregion
        #region Conversions

        public static explicit operator DieRoll(ClampedDieRoll Operand)
            => Operand.GetDieRoll();

        public static explicit operator ClampedDieRoll(DieRoll Operand)
            => new(Operand);

        public static explicit operator InclusiveRange(ClampedDieRoll Operand)
            => InclusiveRange.GetFromDieRoll((DieRoll)Operand);

        public static explicit operator ClampedDieRoll(InclusiveRange Operand)
            => new(Operand);

        #endregion
    }
}
