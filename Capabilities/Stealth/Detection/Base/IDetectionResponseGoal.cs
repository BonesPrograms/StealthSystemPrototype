using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Genkit;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI;
using XRL.World.Parts;

using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Senses;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Capabilities.Stealth.Perception;

namespace StealthSystemPrototype.Detetections
{
    [StealthSystemBaseClass]
    public abstract class IDetectionResponseGoal
        : GoalHandler
    {
        public IOpinionDetection SourceOpinion;

        public ResponseGrammar Grammar;

        protected Type PerceptionType;

        protected IAlertTypedPerception _Perception;
        public virtual IAlertTypedPerception Perception
        {
            get => _Perception ??= ParentObject?.GetPerceptions()?.GetOfType(PerceptionType) as IAlertTypedPerception;
        }

        public IAlert Alert;

        public Cell Origin;

        public AwarenessLevel Level;

        private bool _OverridesCombat;
        public bool OverridesCombat
        {
            get => _OverridesCombat;
            protected set => _OverridesCombat = value;
        }

        public GameObject SourceObject => Source?.GetObject();
        public Cell SourceCell => Source?.GetCurrentCellOrCell();
        public Cell SourceOriginCell => Source?.Cell;

        public bool IsValid => IOpinionDetection.ValidateDetection(this);

        Brain IOpinionDetection.ParentBrain => ParentBrain;

        #region Constructors

        protected IDetectionResponseGoal()
        {
            ID = Guid.Empty;
            Alert = null;
            Origin = null;
            Level = AwarenessLevel.None;
            OverridesCombat = false;
            Source = null;
        }
        protected IDetectionResponseGoal(IPerception Perception, IAlert Alert, AwarenessLevel Level, bool OverridesCombat, DetectionSource Source)
            : this()
        {
            _Perception = Perception;
            PerceptionType = Perception.GetType();
            this.Alert = Alert;
            Origin = Perception.Owner.CurrentCell;
            this.Level = Level;
            this.OverridesCombat = OverridesCombat;
            this.Source = Source?.SetParentDetection(this);
        }
        public IDetectionResponseGoal(AlertContext Context, IAlert Alert, AwarenessLevel Level, bool OverridesCombat)
            : this(Context.Perception, Alert, Level, OverridesCombat, null)
        {
            Source = new DetectionSource(this, Context);
        }
        public IDetectionResponseGoal(IOpinionDetection Source)
            : this(Source.Perception, Source.Alert, Source.Level, Source.OverridesCombat, Source.Source)
        {
        }

        #endregion

        public virtual void Initialize(AlertContext Context)
        {

        }

        public virtual IOpinionDetection<P, A> Copy()
            => new BaseDetection<P, A>(this);

        IOpinionDetection IOpinionDetection.Copy()
            => Copy();

        protected virtual IOpinionDetection<P, A> FromAlertContext(AlertContext Context)
            => Context != null
            ? SetSource(new DetectionSource(this, Context))
            : throw new ArgumentNullException(nameof(Context), "Cannot configure " + Name + " with null " + nameof(AlertContext) + ".");

        protected virtual IOpinionDetection<P, A> SetAlert(A Alert)
        {
            this.Alert = Alert
                ?? throw New_ArgNullException(Alert, nameof(Alert));

            return this;
        }
        protected virtual IOpinionDetection<P, A> SetAwarenessLevel(AwarenessLevel Level)
        {
            if (Level == AwarenessLevel.None)
                throw new ArgumentOutOfRangeException(nameof(Level), Name + " requires an " + nameof(AwarenessLevel) + " greater than " + AwarenessLevel.None.ToStringWithNum() + " to function.");

            this.Level = Level;
            return this;
        }
        protected virtual IOpinionDetection<P, A> SetSource(DetectionSource Source)
        {
            this.Source = Source
                ?? throw New_ArgNullException(Source, nameof(Source));

            return this;
        }
        protected virtual IOpinionDetection<P, A> SetOverridesCombat(bool? OverridesCombat)
        {
            if (OverridesCombat.HasValue)
                this.OverridesCombat = OverridesCombat.GetValueOrDefault();

            return this;
        }
        protected ArgumentNullException New_ArgNullException<T>(T Param, string ParamName)
            => new(ParamName, Name + " requires an " + Param.GetType().ToStringWithGenerics() + " to function.");

        public override void Create()
        {
            base.Create();
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ParentObject?.MiniDebugName()),
                    Debug.Arg(Perception?.ToString(Short: true)),
                });
        }

        public override bool CanFight()
            => !OverridesCombat;

        public override bool Finished()
            => !IsValid;

        public override void TakeAction()
        {
            if (!IsValid)
                FailToParent();

            base.TakeAction();
        }

        #region Explicit Implementations

        IOpinionDetection<P, A> IOpinionDetection<P, A>.FromAlertContext(AlertContext Context)
            => FromAlertContext(Context);

        IOpinionDetection<P, A> IOpinionDetection<P, A>.SetAlert(A Alert)
            => SetAlert(Alert);

        IOpinionDetection<P, A> IOpinionDetection<P, A>.SetAwarenessLevel(AwarenessLevel Level)
            => SetAwarenessLevel(Level);

        IOpinionDetection<P, A> IOpinionDetection<P, A>.SetSource(DetectionSource Source)
            => SetSource(Source);

        IOpinionDetection<P, A> IOpinionDetection<P, A>.SetOverridesCombat(bool? OverridesCombat)
            => SetOverridesCombat(OverridesCombat);

        IOpinionDetection IOpinionDetection.FromAlertContext(AlertContext Context)
            => FromAlertContext(Context);

        IOpinionDetection IOpinionDetection.SetAlert(IAlert Alert)
            => SetAlert(Alert: new A()
            {
                Intensity = Alert.Intensity,
                Properties = Alert.Properties,
            });

        IOpinionDetection IOpinionDetection.SetAwarenessLevel(AwarenessLevel Level)
            => SetAwarenessLevel(Level);

        IOpinionDetection IOpinionDetection.SetSource(DetectionSource Source)
            => SetSource(Source);

        IOpinionDetection IOpinionDetection.SetOverridesCombat(bool? OverridesCombat)
            => SetOverridesCombat(OverridesCombat);

        #endregion
    }
}
