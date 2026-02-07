using System;
using System.Collections.Generic;
using System.Text;

namespace StealthSystemPrototype.Coalescence
{
    [Serializable]
    public enum CoalesceMethod : int
    {
        First,
        Second,
        Greater,
        Lesser,
        Combine,
        Difference,
        TypeDefined,
    }
}
