using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.AI.Pathfinding;

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
    public class OlfactoryPurview
        : BasePurview<Olfactory>
        , IAreaPurview
        , IPathingPurview
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
                    || origin?.GetAdjacentCells(EffectiveValue / 2) is not IEnumerable<Cell> cellsInArea)
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

        private FindPath _LastPath;
        public virtual FindPath LastPath
        {
            get => _LastPath;
            set => _LastPath = value;
        }

        #region Constructors

        public OlfactoryPurview()
            : base()
        {
        }
        public OlfactoryPurview(
            IAlertTypedPerception<Olfactory> ParentPerception,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception as BasePerception, IPurview.DEFAULT_VALUE)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public OlfactoryPurview(
            IAlertTypedPerception<Olfactory> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception as BasePerception, Value)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public OlfactoryPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value)
        {
            _Diffuser = Diffuser ?? _Diffuser;
        }
        public OlfactoryPurview(OlfactoryPurview Source)
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

        public virtual FindPath GetPathTo(AlertContext Context)
            => LastPath = new(
                StartCell: Context?.Perceiver?.CurrentCell,
                EndCell: Context?.AlertLocation,
                Looker: Context?.Perceiver,
                IgnoreCreatures: true);

        public virtual IEnumerable<Cell> GetCellsInArea()
            => AreaCells;

        public override int GetModifedEffectiveLevel(AlertContext Context)
        {
            if (ParentPerception == null
                || !CheckInArea(Context)
                || !CheckCanPathTo(Context, out int steps))
                return 0;

            return (int)Diffuse(steps);
        }

        #region Predicates

        public virtual bool CheckCanPathTo(AlertContext Context, out int Steps)
        {
            Steps = -1;
            if (GetPathTo(Context) is not FindPath findPath
                || !findPath.Found)
                return false;

            List<Cell> steps = findPath.Steps;
            List<int> weights = findPath.Weights;

            int stepsCount = steps.Count;
            int effectiveRangeCents = EffectiveValue * 100;
            for (int i = 0; i < stepsCount; i++)
            {
                if (effectiveRangeCents < 0)
                    return false;

                if (steps[i] == Context.AlertLocation)
                {
                    Steps = i;
                    return true;
                }

                effectiveRangeCents -= weights[i];
            }
            return false;
        }

        public virtual bool CheckInArea(AlertContext Context)
            => AreaCells?.Contains(Context.AlertLocation) ?? false;

        #endregion

        public override void ClearCaches()
        {
            AreaCells = null;
            LastPath = null;
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

        public static explicit operator int(OlfactoryPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
