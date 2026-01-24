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
    public class VisualPurview : BasePurview<Visual>, IPurview<Visual>
    {
        // Purview.RadiusFlags.Line | Purview.RadiusFlags.Occludes | Purview.RadiusFlags.Diffuses
        public static BaseDoubleDiffuser DefaultDiffuser => new DelayedLinearDoubleDiffuser(DelayType.Steps, 5);

        #region Constructors

        protected VisualPurview()
        {
            _ParentPerception = null;
            Value = 0;
        }
        public VisualPurview(int Value)
            : this()
        {
            this.Value = Value;
            Attributes = "Line,Occludes,Diffuses";
        }
        public VisualPurview(int Value, string Attributes)
            : this(Value)
        {
            this.Attributes = Attributes;
        }
        public VisualPurview(IAlertTypedPerception<Visual> ParentPerception, int Value, string Attributes)
            : this(Value, Attributes)
        {
            this.ParentPerception = ParentPerception;
        }
        public VisualPurview(BasePurview<Visual> Source)
            : this(Source.ParentPerception, Source.Value, Source.Attributes)
        {
        }

        #endregion
        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            base.Write(Writer);
            // do writing here
        }
        public override void Read(SerializationReader Reader)
        {
            base.Read(Reader);
            // do reading here
        }

        #endregion

        public override string ToString()
            => base.ToString();

        public override int GetEffectiveValue()
            => base.GetEffectiveValue();

        public override List<string> GetPerviewAttributes()
            => base.GetPerviewAttributes();

        public override int GetPurviewAdjustment(IAlertTypedPerception<Visual> ParentPerception, int Value = 0)
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

        public static explicit operator int(VisualPurview Operand)
            => Operand.EffectiveValue;

        #endregion
    }
}
