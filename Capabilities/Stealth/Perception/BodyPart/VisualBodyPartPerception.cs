using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.Rules;
using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Detetections;
using StealthSystemPrototype.Alerts;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class VisualBodyPartPerception
        : BaseBodyPartPerception<Visual>
        , IAlertTypedPerception<Visual>
    {

        [NonSerialized]
        protected VisualPurview _Purview;
        public IPurview<Visual> Purview
        {
            get => _Purview;
            set
            {
                _Purview = value as VisualPurview;
                _Purview.SetParentPerception(this);
            }
        }
        #region Constructors

        public VisualBodyPartPerception()
            : base()
        {
            SourceType = null;
        }
        public VisualBodyPartPerception(
            GameObject Owner,
            BodyPart Source,
            int Level,
            IPurview Purview)
            : base(Owner, Source, Level, Purview)
        {
        }
        public VisualBodyPartPerception(
            BodyPart Source,
            int Level,
            IPurview Purview)
            : this(Source?.ParentBody?.ParentObject, Source, Level, Purview)
        {
        }

        #endregion
        #region Serialization

        public override IPurview ReadPurview(SerializationReader Reader, IPerception ParentPerception)
        {
            VisualPurview.ReadOptimizedPurview(Reader, out int value, out string attributes);
            return new VisualPurview(this, value, attributes);
        }

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        #endregion

        public IPurview<Visual> GetPurview()
            => throw new NotImplementedException("Better implement this soon!");

        public override bool CanPerceiveAlert(IAlert Alert)
            => Alert != null
            && ((IPerception)this).CanPerceiveAlert(Alert);

        bool IAlertTypedPerception<Visual>.TryPerceive<P>(AlertContext<P, Visual> Context)
            => base.TryPerceive(Context);

        IDetection<P, Visual> IAlertTypedPerception<Visual>.RaiseDetection<P>(AlertContext<P, Visual> Context)
            => throw new NotImplementedException("Better implement this soon!");

        public override BodyPart GetSource()
            => ((IBodyPartPerception)this).GetSource();

        public override bool Validate()
            => base.Validate()
            && Owner.Body?.GetFirstPart(SourceType, false) != null;
    }
}
