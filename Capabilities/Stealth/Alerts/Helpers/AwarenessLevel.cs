using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public enum AwarenessLevel : int
    {
        None,
        Awake,
        Suspect,
        Aware,
        Alert,
    }

    [HasModSensitiveStaticCache]
    public static class AwarenessUtils
    {
        [ModSensitiveStaticCache]
        public static Dictionary<string, AwarenessLevel> ValuesCache = null;

        [ModSensitiveCacheInit]
        public static void CacheAwarenessValues()
        {
            ValuesCache = null;
            Utils.GetValuesDictionary(ref ValuesCache);
        }

        private static AwarenessLevel? _Min;
        public static AwarenessLevel Min => _Min ??= Utils.GetValuesDictionary(ref ValuesCache).Select(kvp => kvp.Value).Min();


        private static AwarenessLevel? _Max;
        public static AwarenessLevel Max => _Max ??= Utils.GetValuesDictionary(ref ValuesCache).Select(kvp => kvp.Value).Max();

        public static bool IsMax(this AwarenessLevel Level)
            => Level == Max;

        public static bool IsMin(this AwarenessLevel Level)
            => Level == Min;

        public static AwarenessLevel Increment(this ref AwarenessLevel Level)
            => !Level.IsMax()
            ? Level++
            : Level;

        public static AwarenessLevel Decrement(this ref AwarenessLevel Level)
            => !Level.IsMin()
            ? Level--
            : Level;

        public static AwarenessLevel Clamp(this ref AwarenessLevel Level, AwarenessLevel Min, AwarenessLevel Max)
        {
            if (Level < Min)
                Level = Min.Clamp();
            else
            if (Level > Max)
                Level = Max.Clamp();
            return Level;
        }

        public static AwarenessLevel Clamp(this ref AwarenessLevel Level)
            => Level.Clamp(Min, Max);
    }
}
