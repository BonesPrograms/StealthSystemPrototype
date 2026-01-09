using System;
using System.Collections.Generic;
using System.Text;

using XRL.World.Anatomy;

namespace StealthSystemPrototype
{
    public static class Utils
    {
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
    }
}
