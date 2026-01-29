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
    public class EsperPurview : PsionicPurview
    {
        #region Constructors

        public EsperPurview()
            : base()
        {
        }
        public EsperPurview(
            IAlertTypedPerception<Psionic, IPurview<Psionic>> ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Value)
        {
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public EsperPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : base(Value, Diffuser)
        {
        }
        public EsperPurview(EsperPurview Source)
            : base(null, Source.Value, Source.Diffuser)
        {
            ParentPerception = Source.ParentPerception;
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
        }

        #endregion

        public override string ToString()
            => base.ToString();

        public override int GetEffectiveValue()
            => base.GetEffectiveValue();

        public override int GetPurviewAdjustment(
            IAlertTypedPerception ParentPerception,
            int Value = 0)
            => base.GetPurviewAdjustment(ParentPerception, Value) + (ParentPerception?.Owner?.Level ?? 0);

        #region Predicates

        public override void ClearCaches()
        {
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

        public static explicit operator int(EsperPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
