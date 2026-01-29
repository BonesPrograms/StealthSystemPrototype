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
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Detetection.Opinions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Capabilities.Stealth.Perception;
using StealthSystemPrototype.Logging;

namespace StealthSystemPrototype.Detetection.ResponseGoals
{
    [StealthSystemBaseClass]
    public abstract class IDetectionResponseGoal : GoalHandler
    {
        protected IOpinionDetection _SourceOpinion;
        public virtual IOpinionDetection SourceOpinion
        {
            get => _SourceOpinion;
            protected set => _SourceOpinion = value;
        }

        protected ResponseGrammar? _Grammar;
        public virtual ResponseGrammar Grammar => _Grammar ??= GetResponseGrammar();

        public AlertContext AlertContext;

        public GameObject Perciever => AlertContext?.Perceiver;

        public GameObject Actor => AlertContext?.Hider;

        public GameObject AlertObject => AlertContext?.AlertObject;

        public Cell Origin;

        public AwarenessLevel Level;

        private bool _OverridesCombat;
        public bool OverridesCombat
        {
            get => _OverridesCombat;
            protected set => _OverridesCombat = value;
        }

        public bool IsValid => Level > AwarenessLevel.None
            && Perciever == ParentObject
            && GameObject.Validate(Perciever)
            && GameObject.Validate(Actor)
            && GameObject.Validate(AlertObject);

        #region Constructors

        public IDetectionResponseGoal()
        {
            SourceOpinion = null;
            _Grammar = null;
            Origin = null;
            Level = AwarenessLevel.None;
            OverridesCombat = false;
        }
        public IDetectionResponseGoal(IOpinionDetection SourceOpinion)
            : this()
        {
            this.SourceOpinion = SourceOpinion;
        }

        #endregion

        public virtual void Initialize(IOpinionDetection SourceOpinion)
        {
            this.SourceOpinion = SourceOpinion;
        }

        public abstract ResponseGrammar GetResponseGrammar();

        public override void Create()
        {
            base.Create();

            AlertContext = SourceOpinion.AlertContext;
            Origin = AlertContext.Perceiver.CurrentCell;

            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(ParentObject?.MiniDebugName()),
                    Debug.Arg(AlertContext.Perception?.ToString(Short: true)),
                });

            string verb = Grammar.Verb ?? GetType().ToStringWithGenerics();
            string theAlertObject = AlertContext.AlertObject.t(Stripped: true, Short: true) ?? "NO_OBJECT";
            string alertLocation = AlertContext?.AlertLocation?.ToString() ?? "NO_LOCATION";
            string levelString = Level.ToString();

            Think("I will " + verb + " the " + nameof(Cell) + " at " + alertLocation +
                " because " + theAlertObject + " there made me " + levelString);
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
    }
}
