using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL.World.Anatomy;

namespace StealthSystemPrototype
{
    public static class Utils
    {
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

        public static Range GetRangeWithOverride(int Start, int End, int? Cap = null)
            => new(Start, Math.Max(Cap ?? End, End));

        public static Range GetRangeWithOverride(Range Range, int? Cap = null)
            => GetRangeWithOverride(Range.Start.Value, Range.End.Value, Cap);

        public static int ClosestBodyPart(BodyPart BodyPart1, BodyPart BodyPart2)
            => EitherNull(BodyPart1, BodyPart2, out int comparison)
            ? comparison
            : BodyPart1.DistanceFromBody().CompareTo(BodyPart2.DistanceFromBody());

        public static int FurthestBodyPart(BodyPart BodyPart1, BodyPart BodyPart2)
            => EitherNull(BodyPart2, BodyPart1, out int comparison)
            ? comparison
            : BodyPart2.DistanceFromBody().CompareTo(BodyPart1.DistanceFromBody());


        public static string CallChain(params string[] Strings)
            => Strings
                ?.Aggregate(
                    seed: "",
                    func: (a, n) => a + (!a.IsNullOrEmpty() && !n.IsNullOrEmpty() ? "." : null) + n);
    }
}
