using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

using XRL;
using XRL.World;
using XRL.World.AI;
using XRL.World.Anatomy;
using XRL.World.Parts;
using XRL.Rules;

using Range = System.Range;

using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Alerts;

using static StealthSystemPrototype.Utils;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype
{
    public static class Extensions
    {
        #region Debug
        [UD_DebugRegistry]
        public static void doDebugRegistry(DebugMethodRegistry Registry)
            => Registry.RegisterEach(
                Type: typeof(StealthSystemPrototype.Extensions),
                MethodNameValues: new Dictionary<string, bool>()
                {
                    { nameof(SetMin), false },
                    { nameof(SetMax), false },
                    { nameof(HasLOSTo), false },
                    { nameof(GetCellsInACosmeticCircleSilent), false },
                });
        #endregion
        #region Clamping

        public static int Clamp(this int Value, int Min, int Max)
            => Math.Clamp(Value, Min, Max);

        public static int Clamp(this int Value, InclusiveRange Range)
            => Value.Clamp(Range.Min, Range.Max);

        public static bool Twixt<T>(this T Value, T Min, T Max, bool Inclusive = false)
            where T : IComparable
            => Inclusive
                ? Value.CompareTo(Min) < 0 && Value.CompareTo(Max) > 0
                : Value.CompareTo(Min) <= 0 && Value.CompareTo(Max) >= 0;

        public static int ToInt<T>(this T Value)
            where T : struct, Enum
            => (int)(object)Value;

        #endregion
        #region Die Rolls

        public static DieRoll AdjustDieCount(this DieRoll DieRoll, int Amount)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(DieRoll), DieRoll),
                    Debug.Arg(nameof(Amount), Amount),
                });

            if (DieRoll.FindType(DieRoll.TYPE_DIE) == null)
                throw new ArgumentException("Must have " + nameof(DieRoll.TYPE_DIE) + " (" + DieRoll.TYPE_DIE + ")", nameof(DieRoll));

            if (DieRoll == null)
                return null;

            int type = DieRoll.Type;
            if (DieRoll.LeftValue > 0)
            {
                DieRoll.LeftValue += Amount;
                return DieRoll;
            }
            else
            {
                if (DieRoll.RightValue > 0)
                    return new(type, DieRoll.Left.AdjustDieCount(Amount), DieRoll.RightValue);
                return new(type, DieRoll.Left.AdjustDieCount(Amount), DieRoll.Right);
            }
        }

        public static bool IsConstantOnlyRangeType(this DieRoll DieRoll)
            => DieRoll.FindTypeWithConstantBoth(DieRoll.TYPE_RANGE) != null;

        public static DieRoll SetMin(this DieRoll DieRoll, int Min)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(DieRoll), DieRoll),
                    Debug.Arg(nameof(Min), Min),
                });

            if (!DieRoll.IsConstantOnlyRangeType())
                throw new ArgumentException("Must have " + nameof(DieRoll.TYPE_RANGE) + " (" + DieRoll.TYPE_RANGE + ")", nameof(DieRoll));

            DieRoll.LeftValue = Min;
            return DieRoll;
        }
        public static bool TrySetMin(this DieRoll DieRoll, int Min, out DieRoll OutDieRoll)
        {
            OutDieRoll = null;
            if (!DieRoll.IsConstantOnlyRangeType())
                return false;

            OutDieRoll = DieRoll.SetMin(Min);
            return true;
        }

        public static DieRoll SetMax(this DieRoll DieRoll, int Max)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(DieRoll), DieRoll),
                    Debug.Arg(nameof(Max), Max),
                });

            if (!DieRoll.IsConstantOnlyRangeType())
                throw new ArgumentException("Must have " + nameof(DieRoll.TYPE_RANGE) + " (" + DieRoll.TYPE_RANGE + ")", nameof(DieRoll));

            DieRoll.RightValue = Max;
            return DieRoll;
        }
        public static bool TrySetMax(this DieRoll DieRoll, int Max, out DieRoll OutDieRoll)
        {
            OutDieRoll = null;
            if (!DieRoll.IsConstantOnlyRangeType())
                return false;

            OutDieRoll = DieRoll.SetMax(Max);
            return true;
        }

        public static DieRoll AdjustMin(this DieRoll DieRoll, int Amount)
            => DieRoll.SetMin(DieRoll.LeftValue + Amount);

        public static bool TryAdjustMin(this DieRoll DieRoll, int Amount, out DieRoll OutDieRoll)
            => DieRoll.TrySetMin(DieRoll.LeftValue + Amount, out OutDieRoll);

        public static DieRoll AdjustMax(this DieRoll DieRoll, int Amount)
            => DieRoll.SetMax(DieRoll.RightValue + Amount);

        public static bool TryAdjustMax(this DieRoll DieRoll, int Amount, out DieRoll OutDieRoll)
            => DieRoll.TrySetMax(DieRoll.RightValue + Amount, out OutDieRoll);

        public static DieRoll AdjustBy(this DieRoll DieRoll, int Amount)
            => DieRoll.AdjustMin(Amount).AdjustMax(Amount);

        public static bool TryAdjustBy(this DieRoll DieRoll, int Amount, out DieRoll OutDieRoll)
            => DieRoll.TryAdjustMin(Amount, out OutDieRoll)
            && OutDieRoll.TryAdjustMax(Amount, out OutDieRoll);

        public static DieRoll Clamp(this DieRoll DieRoll, int Min, int Max)
            => DieRoll.SetMin(DieRoll.LeftValue.Clamp(Min, Max))
                .SetMax(DieRoll.RightValue.Clamp(Min, Max));

        public static DieRoll Clamp(this DieRoll DieRoll, InclusiveRange Range)
            => DieRoll.Clamp(Range.Min, Range.Max);

        public static bool TryClamp(this DieRoll DieRoll, int Min, int Max, out DieRoll OutDieRoll)
            => DieRoll.TrySetMin(DieRoll.LeftValue.Clamp(Min, Max), out OutDieRoll)
            && OutDieRoll.TrySetMax(DieRoll.RightValue.Clamp(Min, Max), out OutDieRoll);

        public static bool TryClamp(this DieRoll DieRoll, InclusiveRange Range, out DieRoll OutDieRoll)
            => DieRoll.TryClamp(Range.Min, Range.Max, out OutDieRoll);

        public static DieRoll ToDieRoll(this Range Range)
            => new(
                Type: DieRoll.TYPE_RANGE,
                Left: Math.Min(Range.Start.GetIntValue(), Range.End.GetIntValue()),
                Right: Math.Max(Range.Start.GetIntValue(), Range.End.GetIntValue()));

        #endregion
        #region Generic Conditionals

        public static bool EqualIncludingBothNull<T>(this T Operand1, T Operand2)
            => (Utils.EitherNull(Operand1, Operand2, out bool areEqual) && areEqual) || (Operand1 != null && Operand1.Equals(Operand2));

        public static bool EqualsAny<T>(this T Value, params T[] args)
            => !args.IsNullOrEmpty()
            && !args.Where(t => t.EqualIncludingBothNull(Value)).IsNullOrEmpty();

        public static bool EqualsAnyNoCase(this string Value, params string[] args)
            => !args.IsNullOrEmpty()
            && !args.Where(t => Value != null && Value.EqualsNoCase(t)).IsNullOrEmpty();

        public static bool EqualsAll<T>(this T Value, params T[] args)
            => !args.IsNullOrEmpty()
            && args.Where(t => t.Equals(Value)).Count() == args.Length;

        public static bool EqualsAllNoCase(this string Value, params string[] args)
            => !args.IsNullOrEmpty()
            && args.Where(t => t.EqualsNoCase(Value)).Count() == args.Length;

        public static bool InheritsFrom(this Type Type, Type OtherType, bool IncludeSelf = true)
            => (IncludeSelf && Type == OtherType)
            || OtherType.IsSubclassOf(Type)
            || Type.IsAssignableFrom(OtherType)
            || (Type.YieldInheritedTypes().ToList() is List<Type> inheritedTypes
                && inheritedTypes.Contains(OtherType));

        public static bool InheritsFrom<T>(this Type Type, bool IncludeSelf = true)
            => Type.InheritsFrom(typeof(T), IncludeSelf);

        public static bool OverlapsWith<T>(this IEnumerable<T> Enumerable1, IEnumerable<T> Enumerable2)
        {
            if (Enumerable1 != null
                && Enumerable2 != null)
                foreach (T item1 in Enumerable1)
                    foreach (T item2 in Enumerable2)
                        if (item1.Equals(item2))
                            return true;

            return false;
        }

        public static bool ContainsAny<T>(this IEnumerable<T> Enumerable, params T[] Items)
            => Items == null
                || Enumerable == null
            ? (Items == null) == (Enumerable == null)
            : Enumerable.OverlapsWith(Items);

        public static bool ContainsAll<T>(this ICollection<T> Collection1, ICollection<T> Collection2)
        {
            int matches = 0;
            int targetNumMatches = Collection2.Count;

            if (targetNumMatches > Collection1.Count)
                return false;

            foreach (T item2 in Collection2)
                foreach (T item1 in Collection1)
                    if (item1.Equals(item2)
                        && targetNumMatches == ++matches)
                        break;

            return targetNumMatches >= matches;
        }
        public static bool ContainsAll<T>(this ICollection<T> Collection, params T[] Items)
            => (Items == null
                || Collection == null)
            ? (Items == null) == (Collection == null)
            : Collection.ContainsAll((ICollection<T>)Items);

        public static bool ContainsAll(this string String, params string[] Strings)
        {
            if (Strings == null || String == null)
                return (Strings == null) == (String == null);

            foreach (string item in Strings)
                if (!String.Contains(item))
                    return false;

            return true;
        }

        public static bool ContainsAny(this string String, params string[] Strings)
        {
            if (Strings == null || String == null)
                return (Strings == null) == (String == null);

            foreach (string item in Strings)
                if (String.Contains(item))
                    return true;

            return false;
        }

        public static bool ContainsNoCase(this string String, string Value)
        {
            if (String.IsNullOrEmpty()
                || Value.IsNullOrEmpty())
                return false;

            int valueIndex = Value.Length - 1;
            string testString = String;
            while (testString.Length >= Value.Length)
            {
                if (testString[..valueIndex].EqualsNoCase(Value))
                    return true;
                testString = testString[1..];
            }
            return false;
        }

        public static bool ContainsAllNoCase(this string String, params string[] Strings)
        {
            if (Strings == null || String == null)
                return (Strings == null) == (String == null);

            foreach (string item in Strings)
                if (!String.ContainsNoCase(item))
                    return false;

            return true;
        }

        public static bool ContainsAnyNoCase(this string String, params string[] Strings)
        {
            if (Strings == null || String == null)
                return (Strings == null) == (String == null);

            foreach (string item in Strings)
                if (String.ContainsNoCase(item))
                    return true;

            return false;
        }

        public static Func<T, bool> ToFunc<T>(this Predicate<T> Filter, bool ThrowIfNull = false)
        {
            if (Filter == null && ThrowIfNull)
                throw new ArgumentNullException(
                    paramName: nameof(Filter),
                    message: "cannot be null if " + nameof(ThrowIfNull) + " is set to " + ThrowIfNull.ToString());

            return Input => Filter == null || Filter(Input);
        }

        public static bool ElementsMatch<Tx, Ty>(this Tx[] X, Ty[] Y)
        {
            if (EitherNullOrEmpty(X, Y, out bool areEqual))
                return areEqual;

            if (X.Length != Y.Length)
                return false;

            for (int i = 0; i < X.Length; i++)
                if (!X[i].Equals(Y[i]))
                    return false;

            return true;
        }

        #endregion
        #region Strings
        public static string MiniDebugName(this GameObject Object)
            => (Object?.ID ?? "#") + ":" + (Object?.GetReferenceDisplayName(WithoutTitles: true, Short: true)?.Strip() ?? "no one");

        public static string ToLiteral(this string String, bool Quotes = false)
        {
            if (String.IsNullOrEmpty())
                return null;

            string output = Microsoft.CodeAnalysis.CSharp.SymbolDisplay.FormatLiteral(String, false);

            if (Quotes)
                output = "\"" + output + "\"";

            return output;
        }

        public static string ThisManyTimes(this string @string, int Times = 1)
        {
            if (Times < 1)
                return null;

            string output = "";

            for (int i = 0; i < Times; i++)
                output += @string;

            return output;
        }
        public static string ThisManyTimes(this char Char, int Times = 1)
            => Char.ToString().ThisManyTimes(Times);

        public static string Join(this string Accumulator, string Next, string Delimiter = ", ")
            => Accumulator + (!Accumulator.IsNullOrEmpty() ? Delimiter : null) + Next;

        public static string Join(this IEnumerable<string> Strings, string Delimiter = ", ")
            => Strings?.Aggregate("", (a, n) => a?.Join(n, Delimiter));

        public static string GenericsString(this IEnumerable<Type> Types, bool Short = false)
            => !Types.IsNullOrEmpty()
            ? "<" + 
                Types
                    .ToList()
                    .ConvertAll(t => t.ToStringWithGenerics(Short))
                    .Join("," + (!Short ? " " : null)) + 
                ">"
            : null;

        public static string ToStringWithGenerics(this Type Type, bool Short = false)
        {
            if (Type == null)
                return null;

            if (Type.GetGenericArguments() is not IEnumerable<Type> typeGenerics)
                return !Short 
                    ? Type.Name
                    : Type.Name.Acronymize();

            string name = Type.Name.Split('`')[0];

            if (Short)
                name = name.Acronymize();

            return name + typeGenerics.GenericsString(Short);
        }

        public static string TypeStringWithGenerics<T>(this T Object, bool Short = false)
            => (Object?.GetType() ?? typeof(T))?.ToStringWithGenerics(Short);

        public static string Acronymize(this string String)
        {
            if (String.IsNullOrEmpty()
                || String.ToLower() == String
                || String.ToUpper() == String)
                return String;

            return String.Aggregate("", (a, n) => a + (char.IsLetter(n) && char.IsUpper(n) ? n : null));
        }

        public static string WithDigits(this double D, int Digits = -1)
            => Digits < 0
            ? D.ToString()
            : String.Format(WithDigitsFormat(Digits), D);

        public static string ToStringWithNum<T>(this T Enum)
            where T : struct, Enum
            => (Enum is int intEnum)
            ? Enum + "(" + intEnum + ")"
            : Enum.ToString();

        public static string SafeJoin<T>(this IEnumerable<T> Enumerable, string Delimiter = ", ")
            => (Enumerable != null
                && Enumerable.Count() > 0)
            ? Enumerable.Aggregate(
                seed: "",
                func: (a, n) => a + (!a.IsNullOrEmpty() ? Delimiter : null) + n.ToString())
            : null;

        public static string ValueUnits(this TimeSpan Duration)
        {
            string durationUnit = "minute";
            double durationValue = Duration.TotalMinutes;
            if (Duration.TotalMinutes < 1)
            {
                durationUnit = "second";
                durationValue = Duration.TotalSeconds;
            }
            if (Duration.TotalSeconds < 1)
            {
                durationUnit = "millisecond";
                durationValue = Duration.TotalMilliseconds;
            }
            if (Duration.TotalMilliseconds < 1)
            {
                durationUnit = "microsecond";
                durationValue = Duration.TotalMilliseconds / 1000;
            }
            return durationValue.Things(durationUnit);
        }

        public static string Signed(this float Float)
            => (Float < 0
                ? null
                : "+") + 
            Float;

        #endregion
        #region Events

        public static bool HasRegisteredEvent(
            this IEnumerable<GameObject> GameObjects,
            Event E,
            out IEnumerable<GameObject> RegisteredGameObjects)
        {
            RegisteredGameObjects = null;
            if (GameObjects.IsNullOrEmpty())
                return false;

            return !(RegisteredGameObjects = GameObjects.Where(GO => GO.HasRegisteredEvent(E.ID))).IsNullOrEmpty();
        }
        public static bool FireEvent(
            this IEnumerable<GameObject> GameObjects,
            Event E,
            bool RegisteredOnly = false)
        {
            if (GameObjects.IsNullOrEmpty())
                return true;

            IEnumerable<GameObject> gameObjects = GameObjects;
            if (RegisteredOnly
                && !gameObjects.HasRegisteredEvent(E, out gameObjects))
                return true;

            foreach (GameObject gameObject in gameObjects)
                if (!gameObject.FireEvent(E))
                    return false;

            return true;
        }

        public static bool WantEvent(
            this IEnumerable<GameObject> GameObjects,
            int ID,
            int Cascade,
            out IEnumerable<GameObject> WantEventGameObjects)
        {
            WantEventGameObjects = null;
            if (GameObjects.IsNullOrEmpty())
                return false;

            return !(WantEventGameObjects = GameObjects.Where(GO => GO.WantEvent(ID, Cascade))).IsNullOrEmpty();
        }
        public static bool HandleEvent<T>(
            this IEnumerable<GameObject> GameObjects,
            T E,
            bool WantOnly = false)
            where T : MinEvent, new()
        {
            if (GameObjects.IsNullOrEmpty())
                return true;

            IEnumerable<GameObject> gameObjects = GameObjects;
            if (WantOnly
                && !gameObjects.WantEvent(E.GetID(), E.GetCascadeLevel(), out gameObjects))
                return true;

            foreach (GameObject gameObject in gameObjects)
                if (!gameObject.HandleEvent(E))
                    return false;

            return true;
        }

        public static Event SetParameterOrNullExisting(
            this Event Event,
            string ParameterName,
            object ParameterValue = null)
        {
            if (Event == null)
                return null;

            if (ParameterValue != null)
                Event.SetParameter(ParameterName, ParameterValue);
            else
            if (Event.HasParameter(ParameterName))
                Event.SetParameter(ParameterName, null);

            return Event;
        }

        #endregion
        #region Anatomy

        public static IEnumerable<BodyPart> LoopPart(this Body Body, string RequiredType, Predicate<BodyPart> Filter)
        {
            foreach (BodyPart bodyPart in Body?.LoopPart(RequiredType) ?? new List<BodyPart>())
                if (Filter == null || Filter(bodyPart))
                    yield return bodyPart;
        }

        public static IEnumerable<BodyPart> LoopPart(this Body Body, string RequiredType, bool ExcludeDismembered)
            => Body?.LoopPart(RequiredType, bp => !ExcludeDismembered || !bp.IsDismembered);

        public static int DistanceFromBody(this BodyPart BodyPart, int StartingDistance = 0)
        {
            if (BodyPart == null
                || BodyPart.ParentPart == null
                || BodyPart.ParentBody == null
                || BodyPart.ParentBody.GetBody() == null)
                return 0;

            if (BodyPart.ParentPart == BodyPart.ParentBody.GetBody())
                return StartingDistance;

            return BodyPart.ParentPart.DistanceFromBody(++StartingDistance);
        }

        #endregion
        #region Brains & Goals

        public static GoalHandler FindGoal(this Brain Brain, Type Type)
        {
            if (Brain?.Goals?.Items is not List<GoalHandler> goalHandlers)
                return null;

            for (int i = goalHandlers.Count - 1; i >= 0; i--)
                if (goalHandlers[i].GetType() == Type)
                    return goalHandlers[i];

            return null;
        }

        public static IAlert FindAlert<TSense>(this Brain Brain, IPerception<TSense> Perception)
            where TSense : ISense<TSense>, new()
        {
            if (Brain?.Goals?.Items is not List<GoalHandler> goalHandlers)
                return null;


            for (int i = goalHandlers.Count - 1; i >= 0; i--)
                if (goalHandlers[i] is IAlert<IPerception<TSense>, TSense> alert
                    && alert.Perception == Perception)
                    return alert;

            return null;
        }

        public static IAlert FirstAlert(this Brain Brain, Predicate<IAlert> Predicate)
            => Brain?.Goals?.Items
                ?.FirstOrDefault(
                    g => g is IAlert gAlert
                    && (Predicate == null || Predicate(gAlert))
                ) as IAlert;

        public static IAlert FirstAlert(this Brain Brain)
            => Brain?.Goals?.Items?.FirstOrDefault(g => g is IAlert) as IAlert;

        public static bool AnyAlert(this Brain Brain, Predicate<IAlert> Predicate)
            => Brain?.Goals?.Items is List<GoalHandler> goalHandlers
            && goalHandlers.Any(g
                => g is IAlert gAlert
                && (Predicate == null
                    || Predicate(gAlert)));

        public static IAlert FindAlert(this Brain Brain, IAlert Alert)
            => Brain.FindGoal(Alert.GetType()) as IAlert;

        #endregion
        #region Cells

        public static bool HasLOSTo(this Cell Cell, Cell OtherCell)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(Cell?.ParentZone?.ZoneID + "[" + (Cell?.Location ?? new(-1,-1)) + "]"),
                    Debug.Arg(OtherCell?.ParentZone?.ZoneID + "[" + (OtherCell?.Location ?? new(-1,-1)) + "]"),
                });

            if (Cell == null
                || OtherCell == null)
                return false;

            if (Cell.ParentZone != OtherCell.ParentZone)
                return false;

            return Cell.ParentZone.CalculateLOS(
                x0: Cell.X,
                y0: Cell.Y,
                x1: OtherCell.X,
                y1: OtherCell.Y,
                BlackoutStops: true);
        }

        private static float GetXD(int x, int X)
            => Math.Abs(x - X);

        private static float GetXD2(int x, int X)
            => GetXD(x, X) * GetXD(x, X);

        private static float GetYD(int y, int Y)
            => (float)(Math.Abs(y - Y) * 1.3333f);

        private static float GetYD2(int y, int Y)
            => GetYD(y, Y) * GetYD(y, Y);

        private static float GetD(int x, int X, int y, int Y)
            => GetXD2(x, X) + GetYD2(y, Y);

        public static IEnumerable<Cell> GetCellsInACosmeticCircleSilent(this Cell Cell, int Radius)
        {
            bool debug = true;
            if (debug)
            {
                using Indent indent = new(1);
                Debug.LogMethod(indent,
                    ArgPairs: new Debug.ArgPair[]
                    {
                        Debug.Arg(nameof(Cell), Cell?.ParentZone?.ZoneID + "[" + (Cell?.Location ?? new(-1,-1)) + "]"),
                        Debug.Arg(nameof(Radius), Radius),
                    });
                debug = false;
            }

            int yRadius = (int)Math.Max(1.0, (double)Radius * 0.66);
            float radiusSquared = Radius * Radius;
            for (int x = Cell.X - Radius; x <= Cell.X + Radius; x++)
                for (int y = Cell.Y - yRadius; y <= Cell.Y + yRadius; y++)
                    if (GetD(x, Cell.X, y, Cell.Y) <= radiusSquared
                        && Cell.ParentZone.GetCell(x, y) is Cell output)
                        yield return output;
        }

        public static IEnumerable<Cell> GetCellsInACosmeticCircle(this Cell Cell, Radius Radius)
            => Cell?.GetCellsInACosmeticCircleSilent(Radius.EffectiveValue);

        #endregion
        #region Collection Manipulation

        public static IEnumerable<T> OrderInPlace<T>(this IEnumerable<T> List, Comparison<T> Comparison)
        {
            List?.ToList()?.Sort(Comparison);
            return List;
        }

        #endregion
        #region Math?

        public static int Average(this int[] Values)
            => !Values.IsNullOrEmpty()
            ? Values.Aggregate(0, (a, n) => a + n) / Values.Length
            : 0;

        #endregion
        #region Serialization

        public static void WriteOptimized(this SerializationWriter Writer, Range Range)
        {
            Writer.WriteOptimized(Range.Start.Value);
            Writer.WriteOptimized(Range.End.Value);
        }

        public static Range ReadOptimizedRange(this SerializationReader Reader)
            => new(Reader.ReadOptimizedInt32(), Reader.ReadOptimizedInt32());

        #endregion
        #region Indices

        public static int GetIntValue(this Index Index)
            => !Index.IsFromEnd
            ? Index.Value
            : -Index.Value;

        #endregion
        #region Ranges

        public static int Sum(this Range Range, bool IncludeIntermediateValues)
            => new InclusiveRange(Range).Sum(IncludeIntermediateValues);

        public static int Sum(this Range Range)
            => Range.Sum(false);

        public static int Average(this Range Range)
            => new InclusiveRange(Range).Average();

        public static int Length(this Range Range)
            => new InclusiveRange(Range).AbsLength;

        public static int Floor(this Range Range)
            => new InclusiveRange(Range).Start;

        public static int Ceiling(this Range Range)
            => new InclusiveRange(Range).End;

        #endregion
        #region InclusiveRanges

        public static DieRoll GetDieRoll(this InclusiveRange InclusiveRange)
            => new(DieRoll.TYPE_RANGE, InclusiveRange.Start, InclusiveRange.End);

        public static int Roll(this InclusiveRange InclusiveRange)
            => Stat.Roll(InclusiveRange.Start, InclusiveRange.End);

        public static int Random(this InclusiveRange InclusiveRange)
            => Stat.Random(InclusiveRange.Start, InclusiveRange.End);

        public static int RandomCosmetic(this InclusiveRange InclusiveRange)
            => Stat.RandomCosmetic(InclusiveRange.Start, InclusiveRange.End);

        public static int SeededRandom(this InclusiveRange InclusiveRange, string Seed)
            => Stat.SeededRandom(Seed, InclusiveRange.Start, InclusiveRange.End);

        #endregion
        #region Comparison

        // nuffin yet.

        #endregion
    }
}
