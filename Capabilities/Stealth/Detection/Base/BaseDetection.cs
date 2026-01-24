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

namespace StealthSystemPrototype.Detetections
{
    [StealthSystemBaseClass]
    [Serializable]
    public class BaseDetection<P, A>
        : GoalHandler
        , IDetection<P, A>
        //, IComparable<BaseDetection>
        where P : class, IAlertTypedPerception<A>, new()
        where A : class, IAlert, new()
    {
        private Guid _ID;
        public Guid ID
        {
            get
            {
                if (_ID == Guid.Empty)
                    _ID = Guid.NewGuid();
                return _ID;
            }
            protected set => _ID = value;
        }

        private string _Name; 
        public string Name => _Name ??= GetType().ToStringWithGenerics();

        private DetectionSource _Source;
        public DetectionSource Source
        {
            get => _Source;
            protected set => _Source = value;
        }

        private DetectionGrammar _Grammar;
        public DetectionGrammar Grammar
        {
            get => _Grammar;
            protected set => _Grammar = value;
        }

        private Type PerceptionType;

        private P _Perception;
        public P Perception
        {
            get => _Perception ??= ParentObject?.GetPerceptions()?.Get<P>();
        }
        IPerception IDetection.Perception => Perception;

        private A _Alert;
        public A Alert
        {
            get => _Alert;
            protected set => _Alert = value;
        }
        IAlert IDetection.Alert => Alert;

        private Cell _Origin;
        public Cell Origin
        {
            get => _Origin;
            protected set => _Origin = value;
        }

        private AwarenessLevel _Level;
        public AwarenessLevel Level
        {
            get => _Level;
            protected set => _Level = value;
        }

        private int _Priority;
        public int Priority
        {
            get => _Priority;
            protected set => _Priority = value;
        }

        private bool _OverridesCombat;
        public bool OverridesCombat
        {
            get => _OverridesCombat;
            protected set => _OverridesCombat = value;
        }

        public GameObject SourceObject => Source?.GetObject();
        public Cell SourceCell => Source?.GetCurrentCellOrCell();
        public Cell SourceOriginCell => Source?.Cell;

        public bool IsValid => IDetection.ValidateDetection(this);

        #region Constructors

        protected BaseDetection()
        {
            ID = Guid.Empty;
            Alert = null;
            Origin = null;
            Level = AwarenessLevel.None;
            OverridesCombat = false;
            Source = null;
        }
        protected BaseDetection(P Perception, A Alert, AwarenessLevel Level, bool OverridesCombat, DetectionSource Source)
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
        public BaseDetection(AlertContext Context, A Alert, AwarenessLevel Level, bool OverridesCombat)
            : this(Context.Perception as P, Alert, Level, OverridesCombat, null)
        {
            Source = new DetectionSource(this, Context);
        }
        public BaseDetection(IDetection Source)
            : this(Source.Perception as P, Source.Alert as A, Source.Level, Source.OverridesCombat, Source.Source)
        {
        }

        #endregion
        #region Serialization

        public virtual void WriteDetection(SerializationWriter Writer, IDetection Detection)
        {
            Writer.Write(ID);
            Writer.WriteComposite(Source);
            Writer.WriteComposite(Grammar);
            Writer.WriteComposite(Alert);
            Writer.Write(Origin);
            Writer.WriteOptimized((int)Level);
            Writer.WriteOptimized(Priority);
            Writer.Write(OverridesCombat);
        }
        public virtual void ReadDetection(SerializationReader Reader, IDetection Detection)
        {
            ID = Reader.ReadGuid();
            Source = Reader.ReadComposite<DetectionSource>();
            Grammar = Reader.ReadComposite<DetectionGrammar>();
            Alert = Reader.ReadComposite<A>();
            Origin = Reader.ReadCell();
            Level = (AwarenessLevel)Reader.ReadOptimizedInt32();
            Priority = Reader.ReadOptimizedInt32();
            OverridesCombat = Reader.ReadBoolean();
        }

        public virtual void Write(SerializationWriter Writer)
        {
            WriteDetection(Writer, this);
        }
        public virtual void Read(SerializationReader Reader)
        {
            ReadDetection(Reader, this);
        }

        #endregion

        public virtual IDetection<P, A> Copy()
            => new BaseDetection<P, A>(this);

        IDetection IDetection.Copy()
            => Copy();

        protected virtual IDetection<P, A> FromAlertContext(AlertContext Context)
            => Context != null
            ? SetSource(new DetectionSource(this, Context))
            : throw new ArgumentNullException(nameof(Context), "Cannot configure " + Name + " with null " + nameof(AlertContext) + ".");

        protected virtual IDetection<P, A> SetAlert(A Alert)
        {
            this.Alert = Alert
                ?? throw New_ArgNullException(Alert, nameof(Alert));

            return this;
        }
        protected virtual IDetection<P, A> SetAwarenessLevel(AwarenessLevel Level)
        {
            if (Level == AwarenessLevel.None)
                throw new ArgumentOutOfRangeException(nameof(Level), Name + " requires an " + nameof(AwarenessLevel) + " greater than " + AwarenessLevel.None.ToStringWithNum() + " to function.");

            this.Level = Level;
            return this;
        }
        protected virtual IDetection<P, A> SetSource(DetectionSource Source)
        {
            this.Source = Source
                ?? throw New_ArgNullException(Source, nameof(Source));

            return this;
        }
        protected virtual IDetection<P, A> SetOverridesCombat(bool? OverridesCombat)
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

        IDetection<P, A> IDetection<P, A>.FromAlertContext(AlertContext Context)
            => FromAlertContext(Context);

        IDetection<P, A> IDetection<P, A>.SetAlert(A Alert)
            => SetAlert(Alert);

        IDetection<P, A> IDetection<P, A>.SetAwarenessLevel(AwarenessLevel Level)
            => SetAwarenessLevel(Level);

        IDetection<P, A> IDetection<P, A>.SetSource(DetectionSource Source)
            => SetSource(Source);

        IDetection<P, A> IDetection<P, A>.SetOverridesCombat(bool? OverridesCombat)
            => SetOverridesCombat(OverridesCombat);

        IDetection IDetection.FromAlertContext(AlertContext Context)
            => FromAlertContext(Context);

        IDetection IDetection.SetAlert(IAlert Alert)
            => SetAlert(Alert: new A()
            {
                Intensity = Alert.Intensity,
                Properties = Alert.Properties,
            });

        IDetection IDetection.SetAwarenessLevel(AwarenessLevel Level)
            => SetAwarenessLevel(Level);

        IDetection IDetection.SetSource(DetectionSource Source)
            => SetSource(Source);

        IDetection IDetection.SetOverridesCombat(bool? OverridesCombat)
            => SetOverridesCombat(OverridesCombat);

        #endregion
    }
}
