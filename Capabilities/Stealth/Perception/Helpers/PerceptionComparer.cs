using System;
using System.Collections.Generic;
using System.Text;

using StealthSystemPrototype.Alerts;
using StealthSystemPrototype;

using static StealthSystemPrototype.Utils;
using XRL.World;
using StealthSystemPrototype.Perceptions.Helpers;

namespace StealthSystemPrototype.Perceptions.Helpers
{
    [Serializable]
    public enum ComparisonType : int
    {
        None,
        Level,
        EffectiveLevel,
        Purview,
    }
}
namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class PerceptionComparer : Comparer<IPerception>, IComposite
    {
        [NonSerialized]
        protected ComparisonType Type;

        protected IAlert Alert;

        public PerceptionComparer()
            : base()
        {
            Alert = null;
            Type = ComparisonType.None;
        }
        public PerceptionComparer(ComparisonType Type, IAlert Alert)
            : this()
        {
            this.Type = Type;
            this.Alert = Alert;
        }
        public PerceptionComparer(ComparisonType Type)
            : this(Type, null)
        { }
        public PerceptionComparer(IAlert Alert)
            : this(ComparisonType.None, Alert)
        { }

        #region Serialization

        public virtual void Write(SerializationWriter Writer)
        {
            Writer.WriteOptimized((int)Type);
        }
        public virtual void Read(SerializationReader Reader)
        {
            Type = (ComparisonType)Reader.ReadOptimizedInt32();
        }

        #endregion

        #region Comparison

        int CompareLevelTo(IPerception x, IPerception y)
            => x.GetLevel() - y.GetLevel();

        int CompareEffectiveLevelTo(IPerception x, IPerception y)
            => x.GetEffectiveLevel() - y.GetEffectiveLevel();

        int ComparePurviewTo(IPerception x, IPerception y)
            => x.GetPurview().CompareTo(y.GetPurview());

        int CompareTo(IPerception x, IPerception y)
        {
            if (EitherNull(x, y, out int comparison))
                return comparison;

            int levelComp = CompareLevelTo(x, y);
            if (levelComp != 0)
                return levelComp;

            int effectiveLevelComp = CompareEffectiveLevelTo(x, y);
            if (effectiveLevelComp != 0)
                return effectiveLevelComp;

            return ComparePurviewTo(x, y);
        }

        #endregion
        public override int Compare(IPerception x, IPerception y)
        {
            if (EitherNull(x, y, out int comparison))
                return comparison;

            if (Alert != null)
            {
                bool xCanPerceive = x.CanPerceive(Alert);
                bool yCanPerceive = y.CanPerceive(Alert);
                int canPerceiveComp = xCanPerceive.CompareTo(yCanPerceive);
                if (canPerceiveComp != 0)
                    return canPerceiveComp;
            }
            return Type switch
            {
                ComparisonType.Level => CompareLevelTo(x, y),
                ComparisonType.EffectiveLevel => CompareEffectiveLevelTo(x, y),
                ComparisonType.Purview => ComparePurviewTo(x, y),
                ComparisonType.None or
                _ => 0,
            };
        }
    }

    public class PerceptionAlertComparer<A> : IComparer<IPerception>
        where A : IAlert
    {
        protected A Alert;

        protected PerceptionAlertComparer()
            => Alert = default;

        public PerceptionAlertComparer(A Alert)
            : this()
            => this.Alert = Alert;

        public virtual int Compare(IPerception x, IPerception y)
        {
            if (EitherNull(x, y, out int nullComp))
                return nullComp;

            if (Alert != null)
            {
            }
            return x.CompareTo(y);
        }
    }

    public class PerceptionAlertContextComparer : IComparer<IPerception>
    {
        protected AlertContext Context;

        protected PerceptionAlertContextComparer()
            => Context = null;

        public PerceptionAlertContextComparer(AlertContext Context)
            : this()
            => this.Context = Context;

        public virtual int Compare(IPerception x, IPerception y)
        {
            if (EitherNull(x, y, out int nullComp))
                return nullComp;

            if (Context != null)
            {
                int canPerceiveComp = x.CanPerceive(Context).CompareTo(y.CanPerceive(Context));
                if (canPerceiveComp != 0)
                    return canPerceiveComp;
            }

            return x.CompareTo(y);
        }
    }
}
