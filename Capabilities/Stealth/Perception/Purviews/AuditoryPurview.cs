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
    public class AuditoryPurview : BasePurview<Auditory>, IPurview<Auditory>
    {
        // Purview.RadiusFlags.Area | Purview.RadiusFlags.Pathing | Purview.RadiusFlags.Diffuses
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        public BaseDoubleDiffuser Diffuser;

        #region Constructors

        protected AuditoryPurview()
            : base()
        {
            Diffuser = null;
        }
        public AuditoryPurview(int Value, BaseDoubleDiffuser Diffuser = null)
            : this()
        {
            this.Value = Value;
            Attributes = "Area,Pathing,Diffuses";
            this.Diffuser = Diffuser ?? DefaultDiffuser;
            this.Diffuser.SetSteps(Value);
        }
        public AuditoryPurview(
            int Value,
            string Attributes,
            BaseDoubleDiffuser Diffuser = null)
            : this(Value, Diffuser)
        {
            this.Attributes = Attributes;
        }
        public AuditoryPurview(
            IAlertTypedPerception<Auditory, AuditoryPurview> ParentPerception,
            int Value,
            string Attributes,
            BaseDoubleDiffuser Diffuser = null)
            : this(Value, Attributes, Diffuser)
        {
            this.ParentPerception = ParentPerception as IAlertTypedPerception<Auditory, IPurview<Auditory>>;
        }
        public AuditoryPurview(AuditoryPurview Source)
            : this(Source.ParentPerception as IAlertTypedPerception<Auditory, AuditoryPurview>, Source.Value, Source.Attributes, Source.Diffuser)
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

        public override int GetPurviewAdjustment(IAlertTypedPerception<Auditory, IPurview<Auditory>> ParentPerception, int Value = 0)
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

        public static explicit operator int(AuditoryPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
