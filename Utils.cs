using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using XRL;
using XRL.World.Anatomy;
using XRL.World.Parts.Mutation;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Const;

using Debug = StealthSystemPrototype.Logging.Debug;
using XRL.Language;
using XRL.World;

namespace StealthSystemPrototype
{
    [HasModSensitiveStaticCache]
    [HasGameBasedStaticCache]
    public static class Utils
    {
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Utils),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(GetFirstCallingModNot), false },
                });
        #endregion
        #region Meta

        public static ModInfo ThisMod => ModManager.GetMod(MOD_ID) ?? ModManager.GetMod(typeof(Utils).Assembly);

        public static ModInfo GetFirstCallingModNot(ModInfo ThisMod)
        {
            try
            {
                Dictionary<Assembly, ModInfo> modAssemblies = ModManager.ActiveMods
                    ?.Where(mi => mi != ThisMod && mi.Assembly != null)
                    ?.ToDictionary(mi => mi.Assembly, mi => mi);

                if (modAssemblies.IsNullOrEmpty())
                {
                    return null;
                }
                StackTrace stackTrace = new();
                for (int i = 0; i < 12 && stackTrace?.GetFrame(i) is StackFrame stackFrameI; i++)
                {
                    if (stackFrameI?.GetMethod() is MethodBase methodBase
                        && methodBase.DeclaringType is Type declaringType
                        && modAssemblies.ContainsKey(declaringType.Assembly))
                    {
                        return modAssemblies[declaringType.Assembly];
                    }
                }
            }
            catch (Exception x)
            {
                MetricsManager.LogException(nameof(GetFirstCallingModNot), x, GAME_MOD_EXCEPTION);
            }
            return null;
        }
        public static bool TryGetFirstCallingModNot(ModInfo ThisMod, out ModInfo FirstCallingMod)
            => (FirstCallingMod = GetFirstCallingModNot(ThisMod)) != null;

        #endregion
        #region Exceptions

        public class InnerArrayNullException : InvalidOperationException
        {
            public InnerArrayNullException(string ParamName)
                : base(ParamName + " is null when it shouldn't be.")
            {
            }
            public InnerArrayNullException()
                : this("An inner array")
            {
            }
        }

        public class CollectionModifiedException : InvalidOperationException
        {
            public CollectionModifiedException(string ParamName)
                : base(ParamName + " was modified; enumeration operation may not execute.")
            {
            }
            public CollectionModifiedException(Type CollectionType)
                : base(CollectionType.Name)
            {
            }
            public CollectionModifiedException()
                : base("Collection")
            {
            }
        }

        #endregion
        #region Comparison

        public class InvertComparison<T> : IComparer<T>
        {
            private readonly Comparer<T> Comparer;
            private InvertComparison()
            {
                Comparer = null;
            }
            public InvertComparison(Comparer<T> Comparer)
            {
                this.Comparer = Comparer;
            }
            public int Compare(T x, T y)
                => Comparer?.Compare(y, x)
                ?? 0;
        }
        public class IndexComparer : IComparer<Index>
        {
            public int Compare(Index x, Index y)
                => x.GetIntValue().CompareTo(y.GetIntValue());
        }
        public class InclusiveRangeComparer : IComparer<InclusiveRange>
        {
            public enum ComparisonType : int
            {
                None,
                Sum,
                Average,
                Length,
                Floor,
                Ceiling,
                FloorThenCeiling,
                CeilingThenFloor,
            }
            public ComparisonType Type;
            public bool Inverted;

            private readonly InclusiveRange X;
            private readonly InclusiveRange Y;

            private InclusiveRangeComparer()
            {
                Type = ComparisonType.None;
                Inverted = false;
                X = default;
                Y = default;
            }
            public InclusiveRangeComparer(ComparisonType Type, bool Inverted = false)
                : this()
            {
                this.Type = Type;
                this.Inverted = Inverted;
            }
            public InclusiveRangeComparer(InclusiveRange X, InclusiveRange Y)
                : this()
            {
                this.X = X;
                this.Y = Y;
            }

            public int Compare(InclusiveRange x, InclusiveRange y)
                => Compare(x, y, Type, Inverted);

            public static int Compare(InclusiveRange x, InclusiveRange y, ComparisonType Type, bool Inverted = false)
            {
                if (Inverted)
                    (x, y) = (y, x);

                return Type switch
                {
                    ComparisonType.Average => CompareAverage(x, y),
                    ComparisonType.Length => CompareLength(x, y),
                    ComparisonType.Floor => CompareFloor(x, y),
                    ComparisonType.Ceiling => CompareCeiling(x, y),
                    ComparisonType.FloorThenCeiling => CompareFloorThenCeiling(x, y),
                    ComparisonType.CeilingThenFloor => CompareCeilingThenFloor(x, y),
                    ComparisonType.Sum or
                    ComparisonType.None or
                    _ => CompareSum(x, y),
                };
            }

            public static int CompareSum(InclusiveRange x, InclusiveRange y)
                => x.Sum() - y.Sum();

            public static int CompareAverage(InclusiveRange x, InclusiveRange y)
                => x.Average() - y.Average();

            public static int CompareLength(InclusiveRange x, InclusiveRange y)
                => x.Length - y.Length;

            public static int CompareFloor(InclusiveRange x, InclusiveRange y)
                => x.Min - y.Min;

            public static int CompareCeiling(InclusiveRange x, InclusiveRange y)
                => x.Max - y.Max;

            public static int CompareFloorThenCeiling(InclusiveRange x, InclusiveRange y)
                => CompareFloor(x, y) is int floorComp
                    && floorComp == 0
                ? CompareCeiling(x, y)
                : floorComp;

            public static int CompareCeilingThenFloor(InclusiveRange x, InclusiveRange y)
                => CompareCeiling(x, y) is int ceilingComp
                    && ceilingComp == 0
                ? CompareFloor(x, y)
                : ceilingComp;

            private bool HasRanges()
                => !X.Equals(default)
                && !Y.Equals(default);

            private bool TryGetRanges(bool Invert, out InclusiveRange X, out InclusiveRange Y)
            {
                X = this.X;
                Y = this.Y;
                if (Invert)
                    (X, Y) = (Y, X);

                return HasRanges();
            }

            public int CompareSum(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareSum(x, y)
                : 0;

            public int CompareAverage(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareAverage(x, y)
                : 0;

            public int CompareBreadth(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareLength(x, y)
                : 0;

            public int CompareFloor(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareFloor(x, y)
                : 0;

            public int CompareCeiling(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareCeiling(x, y)
                : 0;

            public int CompareFloorThenCeiling(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareFloorThenCeiling(x, y)
                : 0;

            public int CompareCeilingThenFloor(bool Invert = false)
                => TryGetRanges(Invert, out InclusiveRange x, out InclusiveRange y)
                ? CompareCeilingThenFloor(x, y)
                : 0;
        }

        public static string GetCloserMatch(string Search, string Current, string Next)
            => Grammar.LevenshteinDistance(Search, Current).CompareTo(Grammar.LevenshteinDistance(Search, Next)) < 0
            ? Current
            : Next;

        #endregion
        #region Strings

        public static string CallChain(params string[] Strings)
            => Strings
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() && !n.IsNullOrEmpty() ? "." : null) + n);

        public static string WithDigitsFormat(int Digits = 0)
            => "{0:0" + (Math.Max(0, Digits) == 0 ? null : "." + "0".ThisManyTimes(Digits)) + "}";

        public static string AppendTick(string String, bool AppendSpace = true)
            => String + "[" + TICK + "]" + (AppendSpace ? " " : "");

        public static string AppendCross(string String, bool AppendSpace = true)
            => String + "[" + CROSS + "]" + (AppendSpace ? " " : "");

        public static string AppendYehNah(string String, bool Yeh, bool AppendSpace = true)
            => String + "[" + (Yeh ? TICK : CROSS) + "]" + (AppendSpace ? " " : "");

        public static string YehNah(bool? Yeh = null)
            => "[" + (Yeh == null ? "-" : (Yeh.GetValueOrDefault() ? TICK : CROSS)) + "]";

        public static string DelimitedAggregator<T>(string Accumulator, T Next, string Delimiter)
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? Delimiter : null) + Next;

        public static string CommaDelimitedAggregator<T>(string Accumulator, T Next)
            => DelimitedAggregator(Accumulator, Next, ",");

        public static string CommaSpaceDelimitedAggregator<T>(string Accumulator, T Next)
            => DelimitedAggregator(Accumulator, Next, ", ");

        public static string NewLineDelimitedAggregator<T>(string Accumulator, T Next)
            => DelimitedAggregator(Accumulator, Next, "\n");

        #endregion
        #region Generic Conditionals

        public static bool EitherNull<Tx, Ty>(Tx X, Ty Y, out bool AreEqual)
        {
            AreEqual = (X is null) == (Y is null);
            return X is null 
                || Y is null;
        }

        public static bool EitherNull<Tx, Ty>(Tx X, Ty Y, out int Comparison)
        {
            Comparison = 0;

            bool xNull = X is null;
            bool yNull = Y is null;

            if (!xNull
                && !yNull)
                return false;

            if (!xNull
                && yNull)
                Comparison = 1;

            if (xNull
                && !yNull)
                Comparison = -1;

            return true;
        }

        public static bool EitherNullOrEmpty<Tx, Ty>(Tx[] X, Ty[] Y, out bool AreEqual)
        {
            if (EitherNull(X, Y, out AreEqual))
                return AreEqual;

            AreEqual = (X.Length > 0) == (Y.Length > 0);
            return X.Length > 0
                || Y.Length > 0;
        }

        #endregion
        #region Predicates

        public static bool HasCustomAttribute<T>(Type Type)
            where T : Attribute
            => Type != null
            && Type.HasCustomAttribute<T>();

        public static bool HasCustomAttribute(Type Type, Type Attribute)
            => Type != null
            && Type.HasCustomAttribute(Attribute);

        public static bool NotHasCustomAttribute<T>(Type Type)
            where T : Attribute
            => Type != null
            && !HasCustomAttribute<T>(Type);

        public static bool NotHasCustomAttribute(Type Type, Type Attribute)
            => Type != null
            && !HasCustomAttribute(Type, Attribute);

        public static bool HasDefaultPublicParameterlessConstructor(Type Type)
            => Type.HasDefaultPublicParameterlessConstructor();

        public static bool IsAbstract(Type Type)
            => Type != null
            && Type.IsAbstract;

        public static bool IsNotAbstract(Type Type)
            => Type != null
            && !IsAbstract(Type);

        public static bool IsMentalWithBaseLevels(BaseMutation BaseMutation)
            => BaseMutation !!= null
            && BaseMutation.IsMental()
            && BaseMutation.BaseLevel > 0;

        #endregion
        #region Math?

        public static int Average(params int[] Values)
            => Values?.Average()
            ?? 0;

        public static void GetMinMax(out int Min, out int Max, params int[] Ints)
        {
            if (Ints.IsNullOrEmpty())
            {
                Min = int.MinValue;
                Max = int.MaxValue;
            }
            else
            if (Ints.Length == 1)
            {
                Min = Ints[0];
                Max = Ints[0];
            }
            else
            {
                Min = int.MaxValue;
                Max = int.MinValue;
                foreach (int value in Ints)
                {
                    Min = Math.Min(Min, value);
                    Max = Math.Min(value, Max);
                }
            }
        }

        #endregion
        #region Anatomy

        public static int ClosestBodyPart(BodyPart BodyPart1, BodyPart BodyPart2)
            => EitherNull(BodyPart1, BodyPart2, out int comparison)
            ? comparison
            : BodyPart1.DistanceFromBody().CompareTo(BodyPart2.DistanceFromBody());

        public static int FurthestBodyPart(BodyPart BodyPart1, BodyPart BodyPart2)
            => EitherNull(BodyPart2, BodyPart1, out int comparison)
            ? comparison
            : BodyPart2.DistanceFromBody().CompareTo(BodyPart1.DistanceFromBody());

        #endregion
        #region Enums

        public static Dictionary<string, T> GetValuesDictionary<T>(ref Dictionary<string, T> CachedValues)
            where T : struct, Enum
        {
            if (CachedValues.IsNullOrEmpty())
            {
                CachedValues ??= new();
                if (Enum.GetValues(typeof(T)) is IEnumerable<T> values)
                    foreach (T value in values)
                        CachedValues[value.ToString()] = value;
            }
            return CachedValues;
        }

        #endregion

        public static Type GetMinEventType(int ID)
            => MinEvent.EventTypes[ID];

    }
}
