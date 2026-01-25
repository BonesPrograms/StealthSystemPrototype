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
    public class OlfactoryPurview : BasePurview<Olfactory>, IPurview<Olfactory>
    {
        // Purview.RadiusFlags.Area | Purview.RadiusFlags.Pathing | Purview.RadiusFlags.Diffuses
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        public BaseDoubleDiffuser Diffuser;

        #region Constructors

        protected OlfactoryPurview()
            : base()
        {
            Diffuser = null;
        }
        public OlfactoryPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this()
        {
            this.Value = Value;
            Attributes = "Area,Pathing,Diffuses";
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public OlfactoryPurview(
            int Value,
            string Attributes,
            BaseDoubleDiffuser Diffuser = null)
            : this(Value, Diffuser)
        {
            this.Attributes = Attributes;
        }
        public OlfactoryPurview(
            IAlertTypedPerception<Olfactory, OlfactoryPurview> ParentPerception,
            int Value,
            string Attributes,
            BaseDoubleDiffuser Diffuser = null)
            : this(Value, Attributes, Diffuser)
        {
            this.ParentPerception = ParentPerception as IAlertTypedPerception<Olfactory, IPurview<Olfactory>>;
        }
        public OlfactoryPurview(OlfactoryPurview Source)
            : this(Source.ParentPerception as IAlertTypedPerception<Olfactory, OlfactoryPurview>, Source.Value, Source.Attributes, Source.Diffuser)
        {
        }
        public OlfactoryPurview(SerializationReader Reader, IAlertTypedPerception<Olfactory, OlfactoryPurview> ParentPerception)
            : base(Reader, ParentPerception as IAlertTypedPerception<Olfactory, IPurview<Olfactory>>)
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

        public override int GetPurviewAdjustment(IAlertTypedPerception<Olfactory, IPurview<Olfactory>> ParentPerception, int Value = 0)
            => base.GetPurviewAdjustment(ParentPerception, Value);

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

        public static explicit operator int(OlfactoryPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
