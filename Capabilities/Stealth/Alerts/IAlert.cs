using System;
using System.Collections.Generic;
using System.Linq;

using XRL;
using XRL.World;

using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Alerts.AlertSet;

namespace StealthSystemPrototype.Alerts
{
    /// <summary>
    /// Contracts a type as being representative of the obviousness of one aspect of an <see cref="IConcealedAction"/> to an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// This serves as a non-generic base which should typically not be derived from directly.
    /// </remarks>
    public interface IAlert
        : ICoalescible<IAlert>
        , ICoalescible<int>
        , ICoalescible
        , IEquatable<IAlert>
        , IEquatable<int>
        , IDisposable
        , IComposite
    {
        #region Static & Cache
        [ModSensitiveStaticCache]
        private static List<Type> _AlertTypes;
        public static IReadOnlyList<Type> AlertTypes
        {
            get => _AlertTypes ??= CacheAlertTypes() as List<Type>;
            private set => _AlertTypes = value as List<Type>;
        }
        [ModSensitiveStaticCache]
        private static List<IAlert> _Alerts;
        public static IReadOnlyList<IAlert> Alerts
        {
            get => _Alerts ??= CacheAlerts() as List<IAlert>;
            private set => _Alerts = value as List<IAlert>;
        }

        [ModSensitiveStaticCache]
        private static Dictionary<string, IAlert> _AlertsByName;
        public static IReadOnlyDictionary<string, IAlert> AlertsByName
        {
            get => _AlertsByName ??= CacheAlertTypesByName() as Dictionary<string, IAlert>;
            private set => _AlertsByName = value as Dictionary<string, IAlert>;
        }

        [ModSensitiveStaticCache]
        private static Dictionary<string, Type> _AlertTypesByName;
        public static IReadOnlyDictionary<string, Type> AlertTypesByName
        {
            get => _AlertTypesByName ??= CacheAlertTypesByName() as Dictionary<string, Type>;
            private set => _AlertTypesByName = value as Dictionary<string, Type>;
        }

        public static IReadOnlyList<Type> CacheAlertTypes(bool ClearFirst = false)
        {
            if (ClearFirst)
                _AlertTypes = null;

            if (!_AlertTypes.IsNullOrEmpty())
                return _AlertTypes;

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            return ModManager.GetTypesAssignableFrom(typeof(IAlert))
                ?.WhereNot(HasCustomAttribute<StealthSystemBaseClassAttribute>)
                ?.WhereNot(IsAbstract)
                ?.Where(HasDefaultPublicParameterlessConstructor)
                ?.ToList();
        }
        public static IReadOnlyList<IAlert> CacheAlerts(bool ClearFirst = false)
        {
            if (ClearFirst)
                _Alerts = null;

            if (!_Alerts.IsNullOrEmpty())
                return _Alerts;

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            if (!AlertTypes.IsNullOrEmpty())
            {
                List<IAlert> alerts = new();
                foreach (Type alertType in AlertTypes)
                {
                    try
                    {
                        using IAlert alertInstance = Activator.CreateInstance(alertType) as IAlert;
                        if (alertInstance != null)
                        {
                            alertInstance.Initialize();
                            alerts.Add(alertInstance);
                            Debug.CheckYeh(alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.CheckYeh(alertType.ToStringWithGenerics(), Indent: indent[1]);
                    }
                    catch (Exception x)
                    {
                        MetricsManager.LogModError(
                            mod: ModManager.GetMod(alertType.Assembly),
                            Message: CallChain(nameof(IAlert), nameof(CacheAlerts)) + ": " +
                                alertType.ToStringWithGenerics() + " didn't like being constructed.\n" + x);

                        indent.SetIndent(0);
                    }
                    indent.SetIndent(0);
                }
                return alerts;
            }
            return null;
        }

        public static IReadOnlyDictionary<string, IAlert> CacheAlertsByName(bool ClearFirst = false)
        {
            if (ClearFirst)
                _AlertsByName = null;

            if (!_AlertsByName.IsNullOrEmpty())
                return _AlertsByName;

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            Dictionary<string, IAlert> alertTypesByName = new();
            if (!Alerts.IsNullOrEmpty())
            {
                foreach (IAlert alert in Alerts)
                    if (alert?.Name is string alertName)
                        alertTypesByName[alertName] = alert;
            }
            else
            if (!AlertTypes.IsNullOrEmpty())
            {
                foreach (Type alertType in AlertTypes)
                {
                    try
                    {
                        if (Activator.CreateInstance(alertType) is IAlert alertInstance
                            && alertInstance?.Name is string alertName)
                        {
                            alertTypesByName[alertName] = alertInstance;
                            Debug.CheckYeh(alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.CheckYeh(alertType.ToStringWithGenerics(), Indent: indent[1]);
                    }
                    catch (Exception x)
                    {
                        MetricsManager.LogModError(
                            mod: ModManager.GetMod(alertType.Assembly),
                            Message: CallChain(nameof(Utils), nameof(CacheAlertsByName)) + ": " +
                                alertType.ToStringWithGenerics() + " didn't like being constructed.\n" + x);

                        indent.SetIndent(0);
                    }
                    indent.SetIndent(0);
                }
                return alertTypesByName;
            }
            return null;
        }

        public static IReadOnlyDictionary<string, Type> CacheAlertTypesByName(bool ClearFirst = false)
        {
            if (ClearFirst)
                _AlertTypesByName = null;

            if (!_AlertTypesByName.IsNullOrEmpty())
                return _AlertTypesByName;

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            Dictionary<string, Type> alertTypesByName = new();
            if (!Alerts.IsNullOrEmpty())
            {
                foreach (IAlert alert in Alerts)
                    if (alert?.Name is string alertName)
                        alertTypesByName[alertName] = alert.GetType();
            }
            else
            if (!AlertsByName.IsNullOrEmpty())
            {
                foreach ((string name, IAlert alert) in AlertsByName)
                    alertTypesByName[name] = alert.GetType();
            }
            else
            if (!AlertTypes.IsNullOrEmpty())
            {
                foreach (Type alertType in AlertTypes)
                {
                    try
                    {
                        using IAlert alertInstance = Activator.CreateInstance(alertType) as IAlert;
                        if (alertInstance?.Name is string alertName)
                        {
                            alertTypesByName[alertName] = alertType;
                            Debug.CheckYeh(alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.CheckYeh(alertType.ToStringWithGenerics(), Indent: indent[1]);
                    }
                    catch (Exception x)
                    {
                        MetricsManager.LogModError(
                            mod: ModManager.GetMod(alertType.Assembly),
                            Message: CallChain(nameof(Utils), nameof(CacheAlertTypesByName)) + ": " +
                                alertType.ToStringWithGenerics() + " didn't like being constructed.\n" + x);

                        indent.SetIndent(0);
                    }
                    indent.SetIndent(0);
                }
                return alertTypesByName;
            }
            return null;
        }

        public static A GetAlert<A>(int Intensity, Dictionary<string, string> Properties = null)
            where A : class, IAlert, new()
        {
            A newAlert = new()
            {
                Intensity = Intensity,
            };
            newAlert.Initialize();
            if (!Properties.IsNullOrEmpty())
                foreach ((string name, string value) in Properties)
                {
                    if (newAlert.Properties.ContainsKey(name))
                        newAlert.Properties[name] += "," + value;
                    else
                        newAlert.Properties[name] = value;
                }
            newAlert.Created();
            return newAlert;
        }

        [ModSensitiveCacheInit]
        public static void InitAlertCache()
        {
            _AlertTypes = CacheAlertTypes(true) as List<Type>;
            _Alerts = CacheAlerts(true) as List<IAlert>;
            _AlertsByName = CacheAlertsByName(true) as Dictionary<string, IAlert>;
            _AlertTypesByName = CacheAlertTypesByName(true) as Dictionary<string, Type>;
        }

        #endregion

        public bool IsBase { get; }

        public string Name { get; }

        public Type Type => GetType();

        public int DefaultIntensity { get; }

        public int Intensity { get; set; }

        public Dictionary<string, string> Properties { get; set; }

        public void Initialize();

        public void Created();

        public IAlert AdjustIntensity(int Amount);

        public IAlert DeepCopy(bool Degrade = false);

        public bool IsType(Type Type);

        public bool IsSame(IAlert Alert);

        public bool IsMatch(AlertContext Context);
    }
}
