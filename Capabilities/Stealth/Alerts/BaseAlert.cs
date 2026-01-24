using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using XRL;
using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Capabilities.Stealth
{
    /// <summary>
    /// Represents the obviousness of an <see cref="IConcealedAction"/> capable of being detected by an appropriate <see cref="IPerception"/>.
    /// </summary>
    /// <remarks>
    /// The degree to which such an activity is detectable.
    /// </remarks>
    [HasModSensitiveStaticCache]
    [StealthSystemBaseClass]
    [Serializable]
    public class BaseAlert : IAlert<BaseAlert>
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

            if (!AlertTypes.IsNullOrEmpty())
            {
                using Indent indent = new();
                Debug.LogCritical(nameof(CacheAlertTypesByName), Indent: indent);
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
                            Debug.LogCritical(YehNah(true) + " " + alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.LogCritical(YehNah(false) + " " + alertType.ToStringWithGenerics(), Indent: indent[1]);
                    }
                    catch (Exception x)
                    {
                        MetricsManager.LogModError(
                            mod: ModManager.GetMod(alertType.Assembly),
                            Message: CallChain(nameof(Utils), nameof(GetSenses)) + ": " +
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

            using Indent indent = new();
            Debug.LogCritical(nameof(CacheAlertTypesByName), Indent: indent);
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
                            Debug.LogCritical(YehNah(true) + " " + alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.LogCritical(YehNah(false) + " " + alertType.ToStringWithGenerics(), Indent: indent[1]);
                    }
                    catch (Exception x)
                    {
                        MetricsManager.LogModError(
                            mod: ModManager.GetMod(alertType.Assembly),
                            Message: CallChain(nameof(Utils), nameof(GetSenses)) + ": " +
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

            using Indent indent = new();
            Debug.LogCritical(nameof(CacheAlertTypesByName), Indent: indent);
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
                            Debug.LogCritical(YehNah(true) + " " + alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.LogCritical(YehNah(false) + " " + alertType.ToStringWithGenerics(), Indent: indent[1]);
                    }
                    catch (Exception x)
                    {
                        MetricsManager.LogModError(
                            mod: ModManager.GetMod(alertType.Assembly),
                            Message: CallChain(nameof(Utils), nameof(GetSenses)) + ": " +
                                alertType.ToStringWithGenerics() + " didn't like being constructed.\n" + x);

                        indent.SetIndent(0);
                    }
                    indent.SetIndent(0);
                }
                return alertTypesByName;
            }
            return null;
        }

        [ModSensitiveCacheInit]
        public void InitAlertCache()
        {
            _AlertTypes = CacheAlertTypes(true) as List<Type>;
            _Alerts = CacheAlerts(true) as List<IAlert>;
            _AlertsByName = CacheAlertsByName(true) as Dictionary<string, IAlert>;
            _AlertTypesByName = CacheAlertTypesByName(true) as Dictionary<string, Type>;
        }

        #endregion
        #region Serialization

        private string _Name;
        public string Name => _Name ??= GetType().ToStringWithGenerics();

        private bool? _IsBase;
        public virtual bool IsBase => _IsBase ??= GetType() == typeof(BaseAlert);

        public virtual int DefaultIntensity => 5;

        private int _Intensity;
        public virtual int Intensity
        {
            get => _Intensity;
            protected set => _Intensity = value;
        }

        private Dictionary<string, string> _Properties;
        public virtual Dictionary<string, string> Properties
        {
            get => _Properties;
            set => _Properties = value;
        }

        #endregion
        #region Constructors

        public BaseAlert()
        {
            _Name = null;
            _IsBase = null;
            _Intensity = DefaultIntensity;
        }
        public BaseAlert(int Intensity)
            : this()
        {
            _Intensity = Intensity;
        }
        public BaseAlert(int Intensity, Dictionary<string, string> Properties)
            : this(Intensity)
        {
            _Properties = Properties;
        }

        #endregion
        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized(_Name);
            Writer.WriteOptimized(_IsBase);
            Writer.WriteOptimized(_Intensity);
            Writer.WriteOptimized(_Properties);
        }

        public virtual void Read(SerializationReader Reader)
        {
            _Name = Reader.ReadOptimizedString();
            _IsBase = Reader.ReadOptimizedNullableBool();
            _Intensity = Reader.ReadOptimizedInt32();
            _Properties = Reader.ReadOptimizedStringPairDictionary();
        }

        #endregion

        public virtual void Initialize()
        {
        }

        public virtual void Created()
        {
        }

        public virtual BaseAlert AdjustIntensity(int Amount)
            => new(Intensity + Amount);

        #region IDisposable

        public virtual void Dispose()
        {
        }

        #endregion
        #region Explicit Implementations

        IAlert IAlert.AdjustIntensity(int Amount)
            => AdjustIntensity(Amount);

        public BaseAlert Copy()
        {
            throw new NotImplementedException();
        }

        IAlert IAlert.Copy()
        {
            return Copy();
        }

        #endregion
    }
}
