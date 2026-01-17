using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

using XRL;

using static StealthSystemPrototype.Utils;
using static StealthSystemPrototype.Const;
using static StealthSystemPrototype.Logging.Debug;
using static StealthSystemPrototype.Logging.Indent;
using System.Linq;
using XRL.World;

namespace StealthSystemPrototype.Logging
{
    [HasModSensitiveStaticCache]
    [HasGameBasedStaticCache]
    public class DebugMethodRegistry : List<MethodRegistryEntry>
    {
        [ModSensitiveStaticCache(CreateEmptyInstance = false)]
        [GameBasedStaticCache(ClearInstance = false)]
        private static DebugMethodRegistry _Instance;
        public static DebugMethodRegistry Instance => _Instance ??= GetRegistry();

        private static bool _GotRegistry = false;

        public DebugMethodRegistry()
        {
        }

        public static DebugMethodRegistry GetRegistry()
        {
            DebugMethodRegistry registry = new();
            if (_GotRegistry)
                return registry;

            try
            {
                List<MethodInfo> debugRegistryMethods = ModManager.GetMethodsWithAttribute(typeof(UD_DebugRegistryAttribute)) ?? new();

                if (debugRegistryMethods.IsNullOrEmpty())
                    MetricsManager.LogModError(
                        mod: ThisMod,
                        Message: nameof(Debug) + "." + nameof(GetRegistry) + " failed to retrieve any " +
                            nameof(UD_DebugRegistryAttribute) + " decorated methods");

                foreach (MethodInfo debugRegistryMethod in debugRegistryMethods)
                    debugRegistryMethod.Invoke(null, new object[] { registry });
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(Debug) + "." + nameof(GetRegistry), x, GAME_MOD_EXCEPTION);
                _GotRegistry = true;
            }
            _GotRegistry = true;
            return registry;
        }


        [ModSensitiveCacheInit]
        [GameBasedCacheInit]
        public static void CacheDoDebugRegistry()
        {
            if (Instance != null)
                LogRegistry();
        }

        public static bool GetDoDebug(string CallingMethod = null)
        {
            if (CallingMethod.IsNullOrEmpty())
                return DoDebug;

            if (Instance is DebugMethodRegistry doDebugRegistry
                && !doDebugRegistry.Any(m => m.GetMethod().Name == CallingMethod))
                return DoDebugSetting;

            return DoDebug;
        }

        public static void LogRegistry()
        {
            UnityEngine.Debug.Log(nameof(Debug) + "." + nameof(LogRegistry));
            if (_GotRegistry)
            {
                foreach (MethodRegistryEntry methodEntry in DoDebugRegistry ?? new())
                    UnityEngine.Debug.Log(methodEntry.ToString());
            }
            else
                UnityEngine.Debug.Log("registry not cached yet");
        }

        public DebugMethodRegistry Register(MethodRegistryEntry RegisterEntry)
        {
            MethodBase methodBase = RegisterEntry.GetMethod();
            string thisMethodName = CallChain(nameof(DebugMethodRegistry), nameof(Register));
            string declaringType = methodBase?.DeclaringType?.Name;
            bool value = RegisterEntry.GetValue();

            if (methodBase == null)
                MetricsManager.LogModWarning(
                    mod: ThisMod,
                    Message: thisMethodName + " passed null " + nameof(MethodBase));

            Add(RegisterEntry);
            UnityEngine.Debug.Log(thisMethodName + "(" + declaringType + "." + (methodBase?.Name ?? "NO_METHOD") + ": " + value + ")");
            return this;
        }

        public DebugMethodRegistry Register<T>(T MethodBase, bool Value)
            where T : MethodBase
            => Register(new MethodRegistryEntry(MethodBase, Value));

        public DebugMethodRegistry Register(
            Type Class,
            string MethodName,
            bool Value)
            => Register(Class?.GetMethod(MethodName), Value);

        public DebugMethodRegistry Register(string MethodName, bool Value)
        {
            string thisMethodName = nameof(Debug) + "." + nameof(Register);
            if (TryGetCallingTypeAndMethod(out Type callingType, out MethodBase callingMethod))
            {
                bool any = false;
                foreach (MethodBase methodBase in callingType.GetMethods() ?? new MethodInfo[0])
                    if (methodBase.Name == MethodName)
                    {
                        any = true;
                        Register(methodBase, Value);
                    }
                if (!any)
                    MetricsManager.LogModWarning(
                        mod: ModManager.GetMod(callingType.Assembly),
                        Message: CallerSignatureString(callingType, callingMethod) +
                            " failed to register any methods called " + MethodName + " with " + thisMethodName);
            }
            else
                MetricsManager.LogModWarning(ThisMod, thisMethodName + " couldn't get " + nameof(callingType));

            return this;
        }

        public DebugMethodRegistry RegisterEachValue(
            Type Type,
            bool Value,
            params string[] Methods)
        {
            if (Methods.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(Methods) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEachValue)));
                return this;
            }
            foreach (MethodBase typeMethod in Type.GetMethods() ?? new MethodBase[0])
                if (Methods.Contains(typeMethod.Name))
                    Register(typeMethod, Value);

            return this;
        }

        public DebugMethodRegistry RegisterEachValue<T>(
            Type Type,
            bool Value,
            params T[] Methods)
            where T : MethodBase
        {
            if (Methods.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(Methods) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEachValue)));
                return this;
            }
            foreach (MethodBase typeMethod in Type.GetMethods() ?? new MethodBase[0])
                if (Methods.Contains(typeMethod))
                    Register(typeMethod, Value);

            return this;
        }

        public DebugMethodRegistry RegisterEach(
            Type Type,
            Dictionary<string, bool> MethodNameValues)
        {
            if (MethodNameValues.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(MethodNameValues) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEach)));
                return this;
            }
            if (MethodNameValues.Values.Any(v => v))
                RegisterEachValue(Type, true, MethodNameValues.Where(e => e.Value).Select(e => e.Key).ToArray());

            if (MethodNameValues.Values.Any(v => !v))
                RegisterEachValue(Type, false, MethodNameValues.Where(e => !e.Value).Select(e => e.Key).ToArray());

            return this;
        }

        public DebugMethodRegistry RegisterEach<T>(
            Type Type,
            Dictionary<T, bool> MethodValues)
            where T : MethodBase
        {
            if (MethodValues.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(MethodValues) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEach)));
                return this;
            }
            if (MethodValues.Values.Any(v => v))
                RegisterEachValue(Type, true, MethodValues.Where(e => e.Value).Select(e => e.Key).ToArray());

            if (MethodValues.Values.Any(v => !v))
                RegisterEachValue(Type, false, MethodValues.Where(e => !e.Value).Select(e => e.Key).ToArray());

            return this;
        }

        public DebugMethodRegistry RegisterEachFalse(
            Type Type,
            params string[] Methods)
        {
            if (Methods.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(Methods) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEachFalse)));
                return this;
            }
            return RegisterEachValue(Type, false, Methods);
        }

        public DebugMethodRegistry RegisterEachFalse<T>(
            Type Type,
            params T[] Methods)
            where T : MethodBase
        {
            if (Methods.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(Methods) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEachFalse)));
                return this;
            }
            return RegisterEachValue(Type, false, Methods);
        }

        public DebugMethodRegistry RegisterEachTrue(
            Type Type,
            params string[] Methods)
        {
            if (Methods.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(Methods) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEachTrue)));
                return this;
            }
            return RegisterEachValue(Type, true, Methods);
        }

        public DebugMethodRegistry RegisterEachTrue<T>(
            Type Type,
            params T[] Methods)
            where T : MethodBase
        {
            if (Methods.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(Methods) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterEachTrue)));
                return this;
            }
            return RegisterEachValue(Type, true, Methods);
        }

        public DebugMethodRegistry RegisterHandleEventVariants(
            Type Type,
            Dictionary<Type, bool> MinEventTypeValues)
        {
            if (MinEventTypeValues.IsNullOrEmpty())
            {
                MetricsManager.LogModWarning(
                    mod: ModManager.GetMod(Type.Assembly),
                    Message: GetCallingTypeAndMethod(ConvertGenerics: true) + " passed empty " + nameof(MinEventTypeValues) + " to " +
                    CallChain(nameof(Debug), nameof(RegisterHandleEventVariants)));
                return this;
            }
            return RegisterEach(
                Type: Type,
                MethodValues: Type.GetMethods()?.Aggregate(
                    seed: new Dictionary<MethodBase, bool>(),
                    func: delegate (Dictionary<MethodBase, bool> a, MethodInfo n)
                    {
                        if (n.Name == nameof(GameObject.HandleEvent)
                            && n.GetParameters() is ParameterInfo[] paramInfos
                            && paramInfos.Length == 1
                            && paramInfos[0].ParameterType is Type eventType
                            && MinEventTypeValues.ContainsKey(eventType))
                            a[n] = MinEventTypeValues[eventType];
                        return a;
                    }));
        }

        public bool Contains<T>(T MethodBase)
            where T : MethodBase
            => MethodBase is not null
            && this.Any(mb => mb.Equals(MethodBase));

        public bool Contains(Type DeclaringType, string MethodName)
            => MethodName is not null
            && this.Any(mb
                => mb.GetMethod() is MethodBase methodBase
                && methodBase.Name == MethodName
                && methodBase.DeclaringType == DeclaringType);

        public bool GetValue<T>(T MethodBase)
            where T : MethodBase
        {
            foreach (MethodRegistryEntry registryEntry in this)
                if (registryEntry.Equals(MethodBase))
                    return (bool)registryEntry;

            throw new ArgumentOutOfRangeException(nameof(MethodBase), "Not found.");
        }

        public bool TryGetValue<T>(
            T MethodBase,
            out bool Value)
            where T : MethodBase
        {
            Value = default;
            if (MethodBase is not null
                && Contains(MethodBase))
            {
                Value = GetValue(MethodBase);
                return true;
            }
            return false;
        }
    }
}
