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
            EsperPsionicPerception ParentPerception,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Diffuser)
        {
        }
        public EsperPurview(
            EsperPsionicPerception ParentPerception,
            int Value,
            BaseDoubleDiffuser Diffuser = null)
            : base(ParentPerception, Value, Diffuser)
        {
        }
        public EsperPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : base(Value, Diffuser)
        {
        }
        public EsperPurview(EsperPurview Source)
            : base(Source)
        {
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

        public override int GetPurviewValueAdjustment(
            BasePerception ParentPerception,
            int Value = 0)
            => base.GetPurviewValueAdjustment(ParentPerception, Value) + (ParentPerception?.Owner?.Level ?? 0);

        public override void ClearCaches()
        {
            base.ClearCaches();
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

        public static explicit operator int(EsperPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
