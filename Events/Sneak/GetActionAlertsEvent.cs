using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using System.Linq;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetActionAlertsEvent : ISneakEvent<GetActionAlertsEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        private BaseConcealedAction ConcealedAction;

        public string ActionID => ConcealedAction?.ID;
        public string ActionName => ConcealedAction?.Name;
        public string Action => ConcealedAction?.Action;

        public List<BaseAlert> ActionAlerts;

        protected List<BaseAlert> RemovedActionAlertsList;

        protected int AdjustAllByLinear;

        protected int AdjustAllByPercent;

        public GetActionAlertsEvent()
            : base()
        {
            ConcealedAction = null;
            ActionAlerts = null;
            RemovedActionAlertsList = null;
            AdjustAllByLinear = 0;
            AdjustAllByPercent = 0;
        }

        public override void Reset()
        {
            base.Reset();
            ConcealedAction = null;
            ActionAlerts = null;
            RemovedActionAlertsList = null;
            AdjustAllByLinear = 0;
            AdjustAllByPercent = 0;
        }

        public static GetActionAlertsEvent FromPool(BaseConcealedAction ConcealedAction)
        {
            if (ConcealedAction.Hider != null
                || FromPool(ConcealedAction.Hider, Performance: ConcealedAction.SneakPerformance) is not GetActionAlertsEvent E)
                return null;

            E.ConcealedAction = ConcealedAction;
            E.ActionAlerts = new(ConcealedAction);
            E.RemovedActionAlertsList = new();

            E.AdjustAllByLinear = 0;
            E.AdjustAllByPercent = 0;

            E.GetStringyEvent();

            return E;
        }

        public override Event GetStringyEvent()
            => base.GetStringyEvent()
                .SetParameterOrNullExisting(nameof(ActionID), ActionID)
                .SetParameterOrNullExisting(nameof(ActionName), ActionName)
                .SetParameterOrNullExisting(nameof(Action), Action)
                .SetParameterOrNullExisting(nameof(ActionAlerts), ActionAlerts)
                ;

        public static void GetFor(BaseConcealedAction ConcealedAction)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(CallChain(nameof(ConcealedAction), nameof(ConcealedAction.ID)), ConcealedAction?.ID ?? "NO_ID"),
                    Debug.Arg(CallChain(nameof(ConcealedAction), nameof(ConcealedAction.Name)), ConcealedAction?.Name ?? "NO_NAME"),
                    Debug.Arg(CallChain(nameof(ConcealedAction), nameof(ConcealedAction.Action)), ConcealedAction?.Action ?? "NO_ACTION"),
                });

            if (ConcealedAction != null
                || Process(FromPool(ConcealedAction), Success: out bool _) is not GetActionAlertsEvent E)
                return;

            ConcealedAction.ReplaceActionAlerts(E.PreviewActionAlerts());
        }

        public IReadOnlyList<BaseAlert> PreviewActionAlerts()
        {
            ActionAlerts ??= new();
            if (ActionAlerts.IsNullOrEmpty())
                return ActionAlerts;

            List<BaseAlert> previewList = new(ActionAlerts);
            foreach (BaseAlert actionAlert in previewList)
            {
                float percent = 1 + AdjustAllByPercent / 100;
                actionAlert.Intensity = (int)(actionAlert.Intensity * percent);
                actionAlert.Intensity += AdjustAllByPercent;
            }
            return previewList;
        }

        public int AdjustAllLinear(int Amount)
            => AdjustAllByLinear += Amount;

        public int AdjustAllPercent(int Amount)
            => AdjustAllByPercent += Amount;

        public int AdjustAllPercent(float Amount)
            => AdjustAllByPercent += (int)(100f * Amount);

        public BaseAlert AddActionAlert(BaseAlert ActionAlert)
        {
            ActionAlerts ??= new();
            if (ActionAlerts.FirstOrDefault(a => a.IsSame(ActionAlert)) is BaseAlert existentAlert)
            {
                ActionAlert.Intensity += existentAlert.Intensity;
                ActionAlerts.Remove(existentAlert);
            }
            ActionAlerts.Add(ActionAlert);

            return RemoveRemovedActionAlert(ActionAlert);
        }

        public A AdjustActionAlert<A>(A ActionAlert, int Amount)
            where A : BaseAlert
        {
            ActionAlerts ??= new();
            if (ActionAlerts.FirstOrDefault(a => a.IsSame(ActionAlert) || a.IsType(typeof(A))) is A actionAlert)
            {
                actionAlert.AdjustIntensity(Amount);
                return actionAlert;
            }
            return null;
        }

        public A AdjustActionAlert<A>(int Amount)
            where A : BaseAlert
            => AdjustActionAlert<A>(null, Amount);

        public A RemoveActionAlert<A>(A ActionAlert)
            where A : BaseAlert
        {
            ActionAlerts ??= new();
            if (ActionAlerts.FirstOrDefault(a => a.IsSame(ActionAlert) || a.IsType(typeof(A))) is A actionAlert
                && ActionAlerts.Remove(actionAlert))
                return AddRemovedActionAlert(actionAlert) as A;

            return null;
        }
        public A RemoveActionAlert<A>()
            where A : BaseAlert
            => RemoveActionAlert<A>(null);

        protected BaseAlert AddRemovedActionAlert(BaseAlert ActionAlert)
        {
            if (RemovedActionAlertsList.FirstOrDefault(a => a.IsSame(ActionAlert)) is BaseAlert existentAlert)
            {
                ActionAlert.Intensity += existentAlert.Intensity;
                RemovedActionAlertsList.Remove(existentAlert);
            }
            RemovedActionAlertsList.Add(ActionAlert);
            return ActionAlert;
        }
        protected BaseAlert RemoveRemovedActionAlert(BaseAlert ActionAlert)
        {
            RemovedActionAlertsList ??= new();
            RemovedActionAlertsList.Remove(RemovedActionAlertsList.FirstOrDefault(a => a.IsSame(ActionAlert)));
            return ActionAlert;
        }

        public IReadOnlyList<BaseAlert> RemovedActionAlerts()
            => RemovedActionAlertsList;
    }
}

