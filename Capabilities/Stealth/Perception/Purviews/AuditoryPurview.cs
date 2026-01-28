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
using StealthSystemPrototype.Alerts;
using XRL.World.AI.Pathfinding;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [Serializable]
    public class AuditoryPurview
        : BasePurview<Auditory>
        , IAreaPurview
        , IPathingPurview
        , IDiffusingPurview
    {
        public static BaseDoubleDiffuser DefaultDiffuser => IDiffusingPurview.DefaultDiffuser;

        public override bool Occludes => false;

        private List<Cell> _AreaCells;
        public virtual List<Cell> AreaCells
        {
            get => _AreaCells ??= ((IAreaPurview)this).GetCellsInArea()?.ToList();
            set => _AreaCells = value;
        }

        private BaseDoubleDiffuser _Diffuser;
        public virtual BaseDoubleDiffuser Diffuser
        {
            get => _Diffuser;
            set => _Diffuser = value;
        }

        private FindPath _LastPath;
        public virtual FindPath LastPath
        {
            get => _LastPath;
            set => _LastPath = value;
        }

        #region Constructors

        public AuditoryPurview()
            : base()
        {
        }
        public AuditoryPurview(
            IAlertTypedPerception<Auditory, IPurview<Auditory>> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Value)
        {
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public AuditoryPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value, Diffuser)
        {
        }
        public AuditoryPurview(AuditoryPurview Source)
            : this(Source.ParentPerception, Source.Value, Source.Diffuser)
        {
        }
        public AuditoryPurview(SerializationReader Reader, IAlertTypedPerception<Auditory, AuditoryPurview> ParentPerception)
            : base(Reader, ParentPerception as IAlertTypedPerception<Auditory, IPurview<Auditory>>)
        {
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
            Diffuser = Reader.ReadComposite() as BaseDoubleDiffuser;
        }

        #endregion

        public override void Construct()
        {
            base.Construct();
            Diffuser ??= GetDefaultDiffuser();
        }

        public override string ToString()
            => base.ToString();

        public override int GetEffectiveValue()
            => base.GetEffectiveValue();

        public override int GetPurviewAdjustment(IAlertTypedPerception<Auditory, IPurview<Auditory>> ParentPerception, int Value = 0)
            => base.GetPurviewAdjustment(ParentPerception, Value);

        public void AssignDefaultDiffuser()
            => Diffuser = GetDefaultDiffuser();

        public virtual BaseDoubleDiffuser GetDefaultDiffuser()
            => DefaultDiffuser.SetSteps(Value) as BaseDoubleDiffuser;

        public virtual double Diffuse(int Value)
        {
            if (Diffuser == null)
                return Value;

            if (Diffuser.TryGetValue(Value, out double diffusedValue))
                return diffusedValue;

            return 0;
        }

        public virtual FindPath GetPathTo(AlertContext Context)
            => ((IPathingPurview)this).GetPathTo(Context);

        public virtual bool CanPathTo(AlertContext Context)
            => ((IPathingPurview)this).CanPathTo(Context);

        #region Predicates

        public override bool IsWithin(AlertContext Context)
            => base.IsWithin(Context);

        public override void ClearCaches()
        {
            AreaCells = null;
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

        public static explicit operator int(AuditoryPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
