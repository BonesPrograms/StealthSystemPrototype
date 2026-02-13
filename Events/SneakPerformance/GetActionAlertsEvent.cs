using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using XRL.World;
using XRL.World.Parts;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.AlertExtensions;
using static StealthSystemPrototype.Capabilities.Stealth.Sneak;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Coalescence;
using XRL.Collections;

namespace StealthSystemPrototype.Events
{
    [GameEvent(Base = true, Cascade = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS, Cache = Cache.Pool)]
    public class GetActionAlertsEvent : ISneakPerformanceEvent<GetActionAlertsEvent>
    {
        public new static readonly int CascadeLevel = CASCADE_EQUIPMENT | CASCADE_INVENTORY | CASCADE_SLOTS;

        private BaseConcealedAction ConcealedAction;

        public string ActionID => ConcealedAction?.ID;
        public string ActionName => ConcealedAction?.Name;
        public string Action => ConcealedAction?.Action;

        protected AlertSet ActionAlerts;

        protected AlertSet RemovedActionAlerts;

        protected Dictionary<string, int> AdjustByPercent;

        protected Dictionary<string, int> AdjustByLinear;

        protected Dictionary<string, int> AdjustByPercentFinal;

        protected int AdjustAllByPercent;

        protected int AdjustAllByLinear;

        public GetActionAlertsEvent()
            : base()
        {
            ConcealedAction = null;
            ActionAlerts = null;
            RemovedActionAlerts = null;
            AdjustByPercent = null;
            AdjustByLinear = null;
            AdjustByPercentFinal = null;
            AdjustAllByPercent = 0;
            AdjustAllByLinear = 0;
        }

        public override void Reset()
        {
            base.Reset();
            ConcealedAction = null;
            ActionAlerts = null;
            RemovedActionAlerts = null;
            AdjustByPercent = null;
            AdjustByLinear = null;
            AdjustByPercentFinal = null;
            AdjustAllByPercent = 0;
            AdjustAllByLinear = 0;
        }

        public static GetActionAlertsEvent FromPool(BaseConcealedAction ConcealedAction)
        {
            if (ConcealedAction.Sneaker != null
                || FromPool(ConcealedAction.Sneaker, Performance: ConcealedAction.SneakPerformance) is not GetActionAlertsEvent E)
                return null;

            E.ConcealedAction = ConcealedAction;
            E.ActionAlerts = new(ConcealedAction);
            E.RemovedActionAlerts = new();

            E.AdjustByPercent = new();
            E.AdjustByLinear = new();
            E.AdjustByPercentFinal = new();

            E.AdjustAllByPercent = 0;
            E.AdjustAllByLinear = 0;

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

            Debug.Log(nameof(E.ActionAlerts), E.ActionAlerts?.Count, Indent: indent[1]);
            Debug.Log(nameof(E.AdjustAllByLinear), E.AdjustAllByLinear, Indent: indent[1]);
            Debug.Log(nameof(E.AdjustAllByPercent), E.AdjustAllByPercent, Indent: indent[1]);

            E.ConcealedAction.ReplaceActionAlerts(E.PreviewActionAlerts());
        }

        public IReadOnlyList<IAlert> PreviewActionAlerts()
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ActionAlerts), ActionAlerts?.Count ?? -1),
                    Debug.Arg(nameof(RemovedActionAlerts), RemovedActionAlerts?.Count ?? -1),
                });

            ActionAlerts ??= new();
            if (ActionAlerts.IsNullOrEmpty())
                return ActionAlerts;

            AdjustByPercent ??= new();
            AdjustByLinear ??= new();
            AdjustByPercentFinal ??= new();

            foreach (var actionAlert in ActionAlerts)
            {
                string alertName = actionAlert.Name;

                if (!AdjustByPercent.ContainsKey(alertName))
                    AdjustByPercent[alertName] = 0;
                AdjustByPercent[alertName] += AdjustAllByPercent;

                if (!AdjustByLinear.ContainsKey(alertName))
                    AdjustByLinear[alertName] = 0;
                AdjustByLinear[alertName] += AdjustAllByLinear;
            }

            using var previewList = ScopeDisposedList<IAlert>.GetFromPoolFilledWith(ActionAlerts);

            foreach (var actionAlert in previewList)
            {
                Debug.Log(actionAlert.Name, actionAlert.Intensity, Indent: indent[1]);

                if (AdjustByPercent.ContainsKey(actionAlert.Name)
                    && AdjustByPercent[actionAlert.Name] is int percentInt)
                {
                    float percent = 1 + percentInt / 100;
                    actionAlert.Intensity = (int)(actionAlert.Intensity * percent);
                    if (percent != 1)
                        Debug.Log(nameof(percent), percent, Indent: indent[2]);
                }
                if (AdjustByLinear.ContainsKey(actionAlert.Name)
                    && AdjustByLinear[actionAlert.Name] is int linearInt)
                    actionAlert.Intensity += linearInt;


                if (AdjustByPercentFinal.ContainsKey(actionAlert.Name)
                    && AdjustByPercentFinal[actionAlert.Name] is int percentFinalInt)
                {
                    float percent = 1 + percentFinalInt / 100;
                    actionAlert.Intensity = (int)(actionAlert.Intensity * percent);
                }
            }
            return previewList;
        }

        public int AdjustAllLinear(int Amount)
            => AdjustAllByLinear += Amount;

        public int AdjustAllPercent(int Amount)
            => AdjustAllByPercent += Amount;

        public int AdjustAllPercent(float Amount)
            => AdjustAllByPercent += (int)(100f * Amount);

        public A AddActionAlert<A>(A ActionAlert)
            where A : class, IAlert, new()
        {
            if (RemovedActionAlerts.IndexOf(ActionAlert) is int removedActionAlertIndex
                && removedActionAlertIndex >= 0)
                RemovedActionAlerts.RemoveAt(removedActionAlertIndex);
            ActionAlerts.Add(ActionAlert);
            return ActionAlert;
        }
        public A AddActionAlert<A>(int Intensity, Dictionary<string, string> Properties = null)
            where A : class, IAlert, new()
            => AddActionAlert(IAlert.GetAlert<A>(Intensity: Intensity, Properties));

        public A AdjustActionAlert<A>(ref Dictionary<string, int> AdjustmentDictionary, A ActionAlert, int Amount)
            where A : class, IAlert, new()
        {
            AdjustmentDictionary ??= new();

            if (ActionAlert != null
                && !ActionAlerts.Any(a => a.IsType(typeof(A))))
                AddActionAlert(ActionAlert);

            using IAlert tempAlert = IAlert.GetAlert<A>(Intensity: 0);
            if (!AdjustmentDictionary.ContainsKey(tempAlert.Name))
                AdjustmentDictionary[tempAlert.Name] = 0;

            AdjustmentDictionary[tempAlert.Name] += Amount;

            return ActionAlerts.FirstOrDefault(a => a.IsType(typeof(A))) as A;
        }

        public A AdjustActionAlertPercent<A>(A ActionAlert, int Amount)
            where A : class, IAlert, new()
            => AdjustActionAlert(ref AdjustByPercent, ActionAlert, Amount)
            ;
        public bool AdjustActionAlertPercent<A>(int Amount)
            where A : class, IAlert, new()
            => AdjustActionAlertPercent<A>(null, Amount) != null;

        public A AdjustActionAlertLinear<A>(A ActionAlert, int Amount)
            where A : class,  IAlert, new()
            => AdjustActionAlert(ref AdjustByLinear, ActionAlert, Amount)
            ;
        public A AdjustActionAlertLinear<A>(int Amount)
            where A : class, IAlert, new()
            => AdjustActionAlertLinear<A>(null, Amount);

        public A AdjustActionAlertPercentFinal<A>(A ActionAlert, int Amount)
            where A : class, IAlert, new()
            => AdjustActionAlert(ref AdjustByPercentFinal, ActionAlert, Amount)
            ;
        public A AdjustActionAlertPercentFinal<A>(int Amount)
            where A : class, IAlert, new()
            => AdjustActionAlertPercentFinal<A>(null, Amount);

        public A RemoveActionAlert<A>(A ActionAlert)
            where A : class,  IAlert, new()
        {
            if (ActionAlerts.Remove(ActionAlert))
                RemovedActionAlerts.Add(ActionAlert);

            return ActionAlert;
        }
        public A RemoveActionAlert<A>()
            where A : class, IAlert, new()
            => RemoveActionAlert<A>(null);

        public IReadOnlyList<IAlert> GetRemovedActionAlerts()
            => RemovedActionAlerts as IReadOnlyList<IAlert>;
    }
}

