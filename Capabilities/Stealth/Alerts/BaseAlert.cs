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
using StealthSystemPrototype.Alerts;

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
    public class BaseAlert : IAlert
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
        private static List<BaseAlert> _Alerts;
        public static IReadOnlyList<BaseAlert> Alerts
        {
            get => _Alerts ??= CacheAlerts() as List<BaseAlert>;
            private set => _Alerts = value as List<BaseAlert>;
        }

        [ModSensitiveStaticCache]
        private static Dictionary<string, BaseAlert> _AlertsByName;
        public static IReadOnlyDictionary<string, BaseAlert> AlertsByName
        {
            get => _AlertsByName ??= CacheAlertTypesByName() as Dictionary<string, BaseAlert>;
            private set => _AlertsByName = value as Dictionary<string, BaseAlert>;
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

            return ModManager.GetTypesAssignableFrom(typeof(BaseAlert))
                ?.WhereNot(HasCustomAttribute<StealthSystemBaseClassAttribute>)
                ?.WhereNot(IsAbstract)
                ?.Where(HasDefaultPublicParameterlessConstructor)
                ?.ToList();
        }
        public static IReadOnlyList<BaseAlert> CacheAlerts(bool ClearFirst = false)
        {
            if (ClearFirst)
                _Alerts = null;

            if (!_Alerts.IsNullOrEmpty())
                return _Alerts;

            if (!AlertTypes.IsNullOrEmpty())
            {
                using Indent indent = new();
                Debug.LogCritical(nameof(CacheAlertTypesByName), Indent: indent);
                List<BaseAlert> alerts = new();
                foreach (Type alertType in AlertTypes)
                {
                    try
                    {
                        using BaseAlert alertInstance = Activator.CreateInstance(alertType) as BaseAlert;
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
                            Message: CallChain(nameof(Utils), nameof(CacheAlerts)) + ": " +
                                alertType.ToStringWithGenerics() + " didn't like being constructed.\n" + x);

                        indent.SetIndent(0);
                    }
                    indent.SetIndent(0);
                }
                return alerts;
            }
            return null;
        }

        public static IReadOnlyDictionary<string, BaseAlert> CacheAlertsByName(bool ClearFirst = false)
        {
            if (ClearFirst)
                _AlertsByName = null;

            if (!_AlertsByName.IsNullOrEmpty())
                return _AlertsByName;

            using Indent indent = new();
            Debug.LogCritical(nameof(CacheAlertTypesByName), Indent: indent);
            Dictionary<string, BaseAlert> alertTypesByName = new();
            if (!Alerts.IsNullOrEmpty())
            {
                foreach (BaseAlert alert in Alerts)
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
                        if (Activator.CreateInstance(alertType) is BaseAlert alertInstance
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
                        using BaseAlert alertInstance = Activator.CreateInstance(alertType) as BaseAlert;
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

        [ModSensitiveCacheInit]
        public void InitAlertCache()
        {
            _AlertTypes = CacheAlertTypes(true) as List<Type>;
            _Alerts = CacheAlerts(true) as List<BaseAlert>;
            _AlertsByName = CacheAlertsByName(true) as Dictionary<string, BaseAlert>;
            _AlertTypesByName = CacheAlertTypesByName(true) as Dictionary<string, Type>;
        }

        #endregion
        #region Instance Fields & Properties

        private string _Name;
        public string Name => _Name ??= GetType().ToStringWithGenerics();

        private bool? _IsBase;
        public virtual bool IsBase => _IsBase ??= GetType() == typeof(BaseAlert);

        public virtual int DefaultIntensity => 5;

        private int _Intensity;
        public virtual int Intensity
        {
            get => _Intensity;
            set => _Intensity = value;
        }

        private Dictionary<string, string> _Properties;
        public virtual Dictionary<string, string> Properties
        {
            get => _Properties;
            set => _Properties = value;
        }

        public Type Type => ((IAlert)this).Type;

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
        public BaseAlert(IAlert Source)
            : this(
                  Intensity: Source.Intensity,
                  Properties: !Source.Properties.IsNullOrEmpty() 
                    ? new(Source.Properties)
                    : new())
        {
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

        public bool IsType(Type Type)
            => Type?.InheritsFrom(GetType()) ?? false;

        public bool IsSame(IAlert Alert)
            => IsType(Alert?.GetType());

        public bool IsMatch(AlertContext Context)
            => IsType(Context?.ActionAlert?.Type);

        public virtual BaseAlert AdjustIntensity(int Amount)
            => new(Intensity + Amount);

        public BaseAlert Copy(bool Degrade)
        {
            BaseAlert baseAlert = Activator.CreateInstance(GetType()) as BaseAlert;
            
            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(baseAlert, fieldInfo.GetValue(this));

            if (Degrade)
            {
                if (baseAlert.Intensity > 0)
                    baseAlert.Intensity--;
                else
                if (baseAlert.Intensity < 0)
                    baseAlert.Intensity++;
            }

            return baseAlert;
        }

        public BaseAlert Copy()
            => Copy(false);

        #region IDisposable

        public virtual void Dispose()
        {
        }

        #endregion
        #region Explicit Implementations

        IAlert IAlert.AdjustIntensity(int Amount)
            => AdjustIntensity(Amount);

        IAlert IAlert.Copy()
            => Copy();

        #endregion
    }
}
