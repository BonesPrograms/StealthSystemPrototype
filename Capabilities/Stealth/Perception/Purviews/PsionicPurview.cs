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
    public class PsionicPurview
        : BasePurview<Psionic>
        , ILinePurview
        , IAreaPurview
        , IDiffusingPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => IDiffusingPurview.DefaultDiffuser;

        public override bool Occludes => false;

        private List<Cell> _AreaCells;
        public virtual List<Cell> AreaCells
        {
            get
            {
                if (_AreaCells.IsNullOrEmpty())
                {
                    if (ParentPerception?.Owner?.CurrentCell is not Cell { InActiveZone: true } origin
                    || origin?.GetAdjacentCells(EffectiveValue) is not IEnumerable<Cell> cellsInArea)
                        return null;

                    _AreaCells = Event.NewCellList(cellsInArea);

                    if (Occludes)
                        _AreaCells.RemoveAll(c => !origin.HasLOSTo(c));
                }
                return _AreaCells;
            }
            set => _AreaCells = value;
        }

        private BaseDoubleDiffuser _Diffuser = DefaultDiffuser;
        public virtual BaseDoubleDiffuser Diffuser => _Diffuser;

        #region Constructors

        public PsionicPurview()
            : base()
        {
        }
        public PsionicPurview(
            IAlertTypedPerception<Psionic> ParentPerception,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception as BasePerception, IPurview.DEFAULT_VALUE)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public PsionicPurview(
            IAlertTypedPerception<Psionic> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception as BasePerception, Value)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public PsionicPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public PsionicPurview(PsionicPurview Source)
            : base(Source)
        {
            _Diffuser = Source.Diffuser ?? _Diffuser;
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.Write(AreaCells);
            Writer.Write(Diffuser);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            AreaCells = Reader.ReadList<Cell>();
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

        public virtual IEnumerable<Cell> GetCellsInArea()
            => AreaCells;

        public override int GetModifedEffectiveLevel(AlertContext Context)
        {
            if (ParentPerception == null
                || !CheckInArea(Context)
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

        public virtual bool CheckInArea(AlertContext Context)
            => AreaCells?.Contains(Context.AlertLocation) ?? false;

        #endregion

        public override void ClearCaches()
        {
            AreaCells = null;
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

        public static explicit operator int(PsionicPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
