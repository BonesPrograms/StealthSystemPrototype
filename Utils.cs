using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Diagnostics;

using XRL;
using XRL.World.Anatomy;

using Range = System.Range;

using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Perceptions;
using static StealthSystemPrototype.Const;

namespace StealthSystemPrototype
{
    public static class Utils
    {
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Utils),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(GetFirstCallingModNot), false },
                });

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
                => x.Value.CompareTo(y.Value);
        }
        public class RangeComparer : IComparer<Range>
        {
            public enum ComparisonType : int
            {
                None,
                Sum,
                Average,
                Breadth,
                Floor,
                Ceiling,
                FloorThenCeiling,
                CeilingThenFloor,
            }
            public ComparisonType Type;
            public bool Inverted;

            private readonly Range X;
            private readonly Range Y;

            private RangeComparer()
            {
                Type = ComparisonType.None;
                Inverted = false;
                X = default;
                Y = default;
            }
            public RangeComparer(ComparisonType Type, bool Inverted = false)
                : this()
            {
                this.Type = Type;
                this.Inverted = Inverted;
            }
            public RangeComparer(Range X, Range Y)
                : this()
            {
                this.X = X;
                this.Y = Y;
            }

            public int Compare(Range x, Range y)
                => Compare(x, y, Type, Inverted);

            public static int Compare(Range x, Range y, ComparisonType Type, bool Inverted = false)
            {
                if (Inverted)
                    (x, y) = (y, x);

                return Type switch
                {
                    ComparisonType.Average => CompareAverage(x, y),
                    ComparisonType.Breadth => CompareBreadth(x, y),
                    ComparisonType.Floor => CompareFloor(x, y),
                    ComparisonType.Ceiling => CompareCeiling(x, y),
                    ComparisonType.FloorThenCeiling => CompareFloorThenCeiling(x, y),
                    ComparisonType.CeilingThenFloor => CompareCeilingThenFloor(x, y),
                    ComparisonType.Sum or
                    ComparisonType.None or
                    _ => CompareSum(x, y),
                };
            }

            public static int CompareSum(Range x, Range y)
                => x.Sum() - y.Sum();

            public static int CompareAverage(Range x, Range y)
                => x.Average() - y.Average();

            public static int CompareBreadth(Range x, Range y)
                => x.Breadth() - y.Breadth();

            public static int CompareFloor(Range x, Range y)
                => x.Floor() - y.Floor();

            public static int CompareCeiling(Range x, Range y)
                => x.Ceiling() - y.Ceiling();

            public static int CompareFloorThenCeiling(Range x, Range y)
                => CompareFloor(x, y) is int floorComp
                    && floorComp == 0
                ? CompareCeiling(x, y)
                : floorComp;

            public static int CompareCeilingThenFloor(Range x, Range y)
                => CompareCeiling(x, y) is int ceilingComp
                    && ceilingComp == 0
                ? CompareFloor(x, y)
                : ceilingComp;

            private bool HasRanges()
                => !X.Equals(default)
                && !Y.Equals(default);

            private bool TryGetRanges(bool Invert, out Range X, out Range Y)
            {
                X = this.X;
                Y = this.Y;
                if (Invert)
                    (X, Y) = (Y, X);

                return HasRanges();
            }

            public int CompareSum(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareSum(x, y)
                : 0;

            public int CompareAverage(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareAverage(x, y)
                : 0;

            public int CompareBreadth(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareBreadth(x, y)
                : 0;

            public int CompareFloor(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareFloor(x, y)
                : 0;

            public int CompareCeiling(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareCeiling(x, y)
                : 0;

            public int CompareFloorThenCeiling(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareFloorThenCeiling(x, y)
                : 0;

            public int CompareCeilingThenFloor(bool Invert = false)
                => TryGetRanges(Invert, out Range x, out Range y)
                ? CompareCeilingThenFloor(x, y)
                : 0;
        }

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

        #endregion
        #region Generic Conditionals

        public static bool EitherNull<T1, T2>(T1 x, T2 y, out bool AreEqual)
        {
            AreEqual = (x is null) == (y is null);
            return x is null 
                || y is null;
        }

        public static bool EitherNull<T1, T2>(T1 x, T2 y, out int Comparison)
        {
            Comparison = 0;
            if ((x is not null)
                && (y is not null))
                return false;

            if (x is not null
                && y is null)
                Comparison = 1;

            if (x is null
                && y is not null)
                Comparison = -1;

            return true;
        }

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

        public static int ClosestBodyPart(BodyPart BodyPart1, BodyPart BodyPart2)
            => EitherNull(BodyPart1, BodyPart2, out int comparison)
            ? comparison
            : BodyPart1.DistanceFromBody().CompareTo(BodyPart2.DistanceFromBody());

        public static int FurthestBodyPart(BodyPart BodyPart1, BodyPart BodyPart2)
            => EitherNull(BodyPart2, BodyPart1, out int comparison)
            ? comparison
            : BodyPart2.DistanceFromBody().CompareTo(BodyPart1.DistanceFromBody());
    }
}
