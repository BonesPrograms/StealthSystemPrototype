using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using XRL;
using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Alerts.AlertSet;

namespace StealthSystemPrototype.Alerts
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
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
        {
            Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Alerts.BaseAlert),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(Copy), false },
                });
        }
        #endregion
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

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

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

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

            if (!AlertTypes.IsNullOrEmpty())
            {
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
                            Debug.CheckYeh(alertInstance.Name, Indent: indent[1]);
                        }
                        else
                            Debug.CheckYeh(alertType.ToStringWithGenerics(), Indent: indent[1]);
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

            using Indent indent = new(1);
            Debug.LogCaller(Indent: indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(ClearFirst), ClearFirst),
                });

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
                        using BaseAlert alertInstance = Activator.CreateInstance(alertType) as BaseAlert;
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
            where A : BaseAlert, new()
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

        private Dictionary<string, string> _Properties = new();
        public virtual Dictionary<string, string> Properties
        {
            get => _Properties;
            set => _Properties = value;
        }

        public Type Type => GetType();

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

        public override string ToString()
            => "<" + Name + ":" + Intensity + ">";

        public virtual void Initialize()
        {
        }

        public virtual void Created()
        {
        }

        public bool IsType(Type Type)
            => Type?.InheritsFrom(GetType()) ?? false;

        public bool IsType<A>()
            where A : BaseAlert, new()
            => IsType(typeof(A));

        public bool IsSame(IAlert Alert)
            => IsType(Alert?.GetType());

        public bool IsMatch(AlertContext Context)
            => IsType(Context?.ActionAlert?.Type);

        public virtual BaseAlert AdjustIntensity(int Amount)
            => new(Intensity + Amount);

        public virtual BaseAlert Copy(bool Degrade)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(GetType().ToStringWithGenerics()),
                    Debug.Arg(nameof(Degrade), Degrade),
                });

            BaseAlert baseAlert = Activator.CreateInstance(GetType()) as BaseAlert;
            Debug.YehNah(nameof(Activator.CreateInstance), baseAlert!= null, Indent: indent[1]);

            FieldInfo[] fields = GetType().GetFields();
            Debug.YehNah(nameof(fields), fields?.Length ?? -1, fields.IsNullOrEmpty(), Indent: indent[1]);

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(baseAlert, fieldInfo.GetValue(this));
            Debug.YehNah(nameof(FieldInfo.SetValue), true, Indent: indent[1]);

            baseAlert.Properties = new();

            if (!Properties.IsNullOrEmpty())
                baseAlert.Properties = new(Properties);
            Debug.YehNah(CallChain(nameof(baseAlert), nameof(baseAlert.Properties)), baseAlert.Properties?.Count ?? -1, baseAlert.Properties.IsNullOrEmpty(), Indent: indent[1]);

            if (Degrade)
            {
                baseAlert.Degrade();
            }
            Debug.YehNah(nameof(Degrade), Degrade, true, Indent: indent[1]);

            return baseAlert;
        }

        public BaseAlert Copy()
            => Copy(false);

        public BaseAlert Degrade(int Amount = 1)
        {
            Intensity = Intensity.TowardZero(Amount);
            return this;
        }

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
