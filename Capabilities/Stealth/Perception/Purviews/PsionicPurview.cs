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
            get => _AreaCells ??= ((IAreaPurview)this).GetCellsInArea()?.ToList();
            set => _AreaCells = value;
        }

        private BaseDoubleDiffuser _Diffuser;
        public virtual BaseDoubleDiffuser Diffuser
        {
            get => _Diffuser;
            set => _Diffuser = value;
        }

        #region Constructors

        public PsionicPurview()
            : base()
        {
        }
        public PsionicPurview(
            IAlertTypedPerception<Psionic, IPurview<Psionic>> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Value)
        {
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public PsionicPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this(null, Value, Diffuser)
        {
        }
        public PsionicPurview(PsionicPurview Source)
            : this(null, Source.Value, Source.Diffuser)
        {
            ParentPerception = Source.ParentPerception;
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

        public static PsionicPurview GetDefaultPerview(IAlertTypedPerception<Psionic, IPurview<Psionic>> ParentPerception = null)
            => new(ParentPerception, 4);

        public override void Construct()
        {
            base.Construct();
            Diffuser ??= GetDefaultDiffuser();
        }

        public override string ToString()
            => base.ToString();

        public override int GetEffectiveValue()
            => base.GetEffectiveValue();

        public override int GetPurviewAdjustment(IAlertTypedPerception ParentPerception, int Value = 0)
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

        public static explicit operator int(PsionicPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
