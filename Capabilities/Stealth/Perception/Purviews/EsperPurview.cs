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
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Capabilities.Stealth.Perception
{
    [Serializable]
    public class EsperPurview : PsionicPurview
    {
        // Purview.RadiusFlags.Line | Purview.RadiusFlags.Area | Purview.RadiusFlags.Diffuses
       
        #region Constructors

        protected EsperPurview()
            : base()
        {
            _ParentPerception = null;
            Value = 0;
            Diffuser = null;
        }
        public EsperPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this()
        {
            this.Value = Value;
            Attributes = "Line,Area,Diffuses";
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public EsperPurview(
            int Value,
            string Attributes,
            BaseDoubleDiffuser Diffuser = null)
            : this(Value, Diffuser)
        {
            this.Attributes = Attributes;
        }
        public EsperPurview(
            IAlertTypedPerception<Psionic, EsperPurview> ParentPerception,
            int Value,
            string Attributes,
            BaseDoubleDiffuser Diffuser = null)
            : this(Value, Attributes, Diffuser)
        {
            this.ParentPerception = ParentPerception as IAlertTypedPerception<Psionic, IPurview<Psionic>>;
        }
        public EsperPurview(PsionicPurview Source)
            : this(Source.ParentPerception as IAlertTypedPerception<Psionic, EsperPurview>, Source.Value, Source.Attributes, Source.Diffuser)
        {
        }
        public EsperPurview(SerializationReader Reader, IAlertTypedPerception<Psionic, EsperPurview> ParentPerception)
            : base(Reader, ParentPerception as IAlertTypedPerception<Psionic, PsionicPurview>)
        {
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            Writer.Write(Diffuser);
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            Diffuser = Reader.ReadComposite() as BaseDoubleDiffuser;
        }

        #endregion

        public override string ToString()
            => base.ToString();

        public override int GetEffectiveValue()
            => base.GetEffectiveValue();

        public override List<string> GetPerviewAttributes()
            => base.GetPerviewAttributes();

        public override int GetPurviewAdjustment(
            IAlertTypedPerception<Psionic, IPurview<Psionic>> ParentPerception,
            int Value = 0)
            => base.GetPurviewAdjustment(ParentPerception, Value) + (ParentPerception?.Owner?.Level ?? 0);

        #region Predicates

        public override bool HasAttribute(string Attribute)
            => base.HasAttribute(Attribute);

        public override bool HasAttributes(params string[] Attributes)
            => base.HasAttributes(Attributes);

        public override bool IsWithin(Cell Cell)
            => base.IsWithin(Cell);

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
