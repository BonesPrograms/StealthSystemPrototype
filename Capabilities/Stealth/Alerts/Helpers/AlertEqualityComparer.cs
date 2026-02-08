using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Coalescence;
using static StealthSystemPrototype.Utils;

namespace StealthSystemPrototype.Alerts
{
    [Serializable]
    public class AlertEqualityComparer : EqualityComparer<IAlert>
    {
        [Serializable]
        [Flags]
        public enum EqualityComparisonType : int
        {
            Reference,
            Value,
            Type,
        }

        protected EqualityComparisonType _ComparisonTypeFlags;
        public EqualityComparisonType ComparisonTypeFlags => _ComparisonTypeFlags;

        public AlertEqualityComparer()
            : base()
        {
            _ComparisonTypeFlags = EqualityComparisonType.Type;
        }
        public AlertEqualityComparer(EqualityComparisonType ComparisonTypeFlags)
            : this()
        {
            _ComparisonTypeFlags = ComparisonTypeFlags;
        }

        protected bool EqualsValue(IAlert x, IAlert y)
            => x.Intensity == y.Intensity;

        protected bool EqualsType(IAlert x, IAlert y)
            => x.Type == y.Type;

        protected bool EqualsReference(IAlert x, IAlert y)
            => x == y;

        public override bool Equals(IAlert x, IAlert y)
        {
            if (EitherNull(x, y, out bool areEqual))
                return areEqual;

            if (ComparisonTypeFlags.HasFlag(EqualityComparisonType.Reference))
                return EqualsReference(x, y);

            if (ComparisonTypeFlags.HasFlag(EqualityComparisonType.Type)
                && !EqualsType(x, y))
                return false;

            if (ComparisonTypeFlags.HasFlag(EqualityComparisonType.Value)
                && !EqualsValue(x, y))
                return false;

            return true;
        }

        public override int GetHashCode(IAlert obj)
        {
            throw new NotImplementedException();
        }
    }
}
