using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World;
using XRL.World.Anatomy;
using XRL.World.Parts;

namespace StealthSystemPrototype
{
    public static class Extensions
    {
        public static int Clamp(this int Value, int Min, int Max)
            => Math.Clamp(Value, Min, Max);

        public static int Clamp(this int Value, Range Range)
            => Value.Clamp(Range.Start.Value, Range.End.Value);

        public static int ClampWithCap(this int Value, int Min, int Max, int? Cap = null)
            => Value.Clamp(Utils.GetRangeWithOverride(Min, Max, Cap));

        public static int ClampWithCap(this int Value, Range Range, int? Cap = null)
            => Value.Clamp(Utils.GetRangeWithOverride(Range, Cap));

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

        public static bool InheritsFrom(this Type T, Type Type, bool IncludeSelf = true)
            => (IncludeSelf && T == Type)
            || Type.IsSubclassOf(T)
            || T.IsAssignableFrom(Type)
            || (T.YieldInheritedTypes().ToList() is List<Type> inheritedTypes
                && inheritedTypes.Contains(Type));

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

        #endregion

        #region Strings

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

            return Type.Name.Split('`')[0] + typeGenerics.GenericsString(Short);
        }

        public static string Acronymize(this string String)
        {
            if (String.IsNullOrEmpty()
                || String.ToLower() == String
                || String.ToUpper() == String)
                return String;

            return String.Aggregate("", (a, n) => a + (char.IsLetter(n) && char.IsUpper(n) ? n : null));
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

        #region Cells

        public static bool HasLOSTo(this Cell Cell, Cell OtherCell)
        {
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

        #endregion

        #region Collection Manipulation

        public static IEnumerable<T> OrderInPlace<T>(this IEnumerable<T> List, Comparison<T> Comparison)
        {
            List?.ToList()?.Sort(Comparison);
            return List;
        }

        #endregion
    }
}
