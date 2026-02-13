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
using XRL.Collections;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [Serializable]
    public class AreaPathDiffusePurview
        : BasePurview
        , IAreaPurview
        , IPathingPurview
        , IDiffusingPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => IDiffusingPurview.DefaultDiffuser;

        public override bool Occludes => false;

        protected BaseDoubleDiffuser _Diffuser = null;
        public virtual BaseDoubleDiffuser Diffuser
        {
            get => _Diffuser ??= DefaultDiffuser;
            protected set => _Diffuser = value;
        }

        protected Cell _Origin = null;
        public Cell Origin => _Origin;

        #region Constructors

        protected AreaPathDiffusePurview()
            : base()
        { }
        public AreaPathDiffusePurview(
            int Value,
            Cell Origin,
            BaseDoubleDiffuser Diffuser = null)
            : base(Value)
        {
            _Origin = Origin;
            _Diffuser = Diffuser;
        }
        public AreaPathDiffusePurview(int BaseValue, Cell Origin)
            : this(BaseValue, Origin, DefaultDiffuser)
        { }
        public AreaPathDiffusePurview(AreaPathDiffusePurview Source)
            : this(Source.BaseValue, Source.Origin, Source.Diffuser)
        { }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.Write(Diffuser);
            Writer.Write(Origin);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            _Diffuser = Reader.ReadComposite() as BaseDoubleDiffuser;
            _Origin = Reader.ReadCell();
        }

        #endregion

        public override string ToString()
            => base.ToString();

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
        public virtual double Diffuse(int Level)
        {
            BaseDoubleDiffuser diffuser = Diffuser ?? DefaultDiffuser;
            if (diffuser == null)
                return Level;

            diffuser.SetSteps(BaseValue);

            if (!diffuser.TryGetValue(BaseValue, out double diffusionFactor))
                return 0;

            return Level * diffusionFactor;
        }

        public virtual FindPath GetPathTo(ref PerceptionSet.AlertEvent E)
            => new(
                StartCell: Origin,
                EndCell: E.AlertLocation,
                Looker: E.Perceiver,
                IgnoreCreatures: true);

        public virtual IEnumerable<Cell> GetCellsInArea()
        {
            if (Origin == null)
                return null;

            if (Origin?.GetAdjacentCells(BaseValue) is not IEnumerable<Cell> cellsInArea)
                return null;

            if (Occludes)
                return cellsInArea;

            return cellsInArea?.Where(c => Origin.HasLOSTo(c));
        }

        public override int GetEffectiveLevel(IPerception Perception, ref PerceptionSet.AlertEvent E)
        {
            if (Perception == null
                || !CheckInArea(ref E)
                || !CheckCanPathTo(ref E, out int steps))
                return 0;

            return (int)Diffuse(steps);
        }

        #region Predicates

        public virtual bool CheckCanPathTo(ref PerceptionSet.AlertEvent E, out int Steps)
        {
            Steps = -1;
            if (GetPathTo(ref E) is not FindPath findPath
                || !findPath.Found)
                return false;

            using var steps = ScopeDisposedList<Cell>.GetFromPoolFilledWith(findPath.Steps);
            using var weights = ScopeDisposedList<int>.GetFromPoolFilledWith(findPath.Weights);

            int stepsCount = steps.Count;
            int effectiveRangeCents = BaseValue * 100;
            for (int i = 0; i < stepsCount; i++)
            {
                if (effectiveRangeCents < 0)
                    return false;

                if (steps[i] == E.AlertLocation)
                {
                    Steps = i;
                    return true;
                }
                effectiveRangeCents -= weights[i];
            }
            return false;
        }

        public virtual bool CheckInArea(ref PerceptionSet.AlertEvent E)
            => GetCellsInArea() is IEnumerable<Cell> areaCells
            && areaCells.Contains(E.AlertLocation);

        #endregion
    }
}
