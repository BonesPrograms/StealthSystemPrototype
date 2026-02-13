using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Perceptions.Helpers;
using StealthSystemPrototype.Perceptions;

using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Perceptions.Helpers
{
    [Serializable]
    [Flags]
    public enum EqualityComparisonType : int
    {
        Reference = 0,
        Value = 1,
        AlertType = 2,
    }
}
namespace StealthSystemPrototype.Perceptions
{
    public class PerceptionEqualityComparer : EqualityComparer<IPerception>
    {

        protected EqualityComparisonType _ComparisonTypeFlags;
        public EqualityComparisonType ComparisonTypeFlags => _ComparisonTypeFlags;

        public PerceptionEqualityComparer()
            : base()
        {
            _ComparisonTypeFlags = EqualityComparisonType.AlertType;
        }
        public PerceptionEqualityComparer(EqualityComparisonType ComparisonTypeFlags)
            : this()
        {
            _ComparisonTypeFlags = ComparisonTypeFlags;
        }

        protected bool EqualsValue(IPerception x, IPerception y)
            => x.Level == y.Level;

        protected bool EqualsType(IPerception x, IPerception y)
            => x.GetAlertType() == y.GetAlertType();

        protected bool EqualsReference(IPerception x, IPerception y)
            => x == y;

        public override bool Equals(IPerception x, IPerception y)
        {
            if (EitherNull(x, y, out bool areEqual))
                return areEqual;

            if (ComparisonTypeFlags.HasFlag(EqualityComparisonType.Reference))
                return EqualsReference(x, y);

            if (ComparisonTypeFlags.HasFlag(EqualityComparisonType.AlertType)
                && !EqualsType(x, y))
                return false;

            if (ComparisonTypeFlags.HasFlag(EqualityComparisonType.Value)
                && !EqualsValue(x, y))
                return false;

            return true;
        }

        public override int GetHashCode(IPerception obj)
            => obj.GetType().GetHashCode()
            ^ obj.Level.GetHashCode();
    }
}
