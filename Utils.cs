using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype
{
    public static class Utils
    {
        public static bool EitherNull<T1, T2>(T1 x, T2 y, out bool AreEqual)
        {
            AreEqual = (x is null) == (y is null);
            if (x is null || y is null)
                return true;

            return false;
        }

        public static bool EitherNull<T1, T2>(T1 x, T2 y, out int Comparison)
        {
            Comparison = 0;
            if (x is not null && y is null)
            {
                Comparison = 1;
                return true;
            }
            if (x is null && y is not null)
            {
                Comparison = -1;
                return true;
            }
            if ((x is null) && (y is null))
            {
                Comparison = 0;
                return true;
            }
            return false;
        }
    }
}
