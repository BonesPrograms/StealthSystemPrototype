using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using XRL;
using XRL.Rules;
using XRL.World;
using XRL.World.AI.Pathfinding;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;

using static StealthSystemPrototype.Utils;
using System.Reflection;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class IPerception<TSense>
        : IPerception,
        IComparable<IPerception<TSense>>
        where TSense : ISense<TSense>, new()
    {
        #region Helpers

        public class TypedRatingComparer : IComparer<IPerception<TSense>>
        {
            protected GameObject Entity;

            private TypedRatingComparer()
            {
                Entity = null;
            }
            public TypedRatingComparer(GameObject Entity)
                : base()
            {
                this.Entity = Entity;
            }

            public virtual int Compare(IPerception<TSense> x, IPerception<TSense> y)
                => x.Sense == y.Sense
                ? new RatingComparer(Entity).Compare(x, y)
                : 0;
        }

        #endregion
        #region Const & Static Values

        // Add as necessary.

        #endregion
        #region Instance Fields & Properties

        public override ClampedDieRoll DieRoll => _DieRoll ??= GetDieRoll(this, new TSense());
        public override Radius Radius => _Radius ??= GetRadius(this, new TSense());

        #endregion
        #region Constructors

        public IPerception()
            : base()
        {
        }
        public IPerception(GameObject Owner)
            : base(Owner)
        {
        }
        public IPerception(
            GameObject Owner,
            ClampedDieRoll BaseDieRoll,
            Radius BaseRadius)
            : base(Owner, BaseDieRoll, BaseRadius)
        {
        }

        #endregion
        #region Serialization

        public override void Write(GameObject Basis, SerializationWriter Writer)
        {
            base.Write(Basis, Writer);
            // do writing here
        }
        public override void Read(GameObject Basis, SerializationReader Reader)
        {
            base.Read(Basis, Reader);
            // do reading here
        }

        public override void FinalizeRead(SerializationReader Reader)
        {
            // do reading finalization here
        }

        #endregion
        #region Base Methods


        public override void Initialize()
        {
        }

        public override void Attach()
        {
        }

        public override void AddedAfterCreation()
        {
        }

        public override void Remove()
        {
        }

        public override IPerception DeepCopy(GameObject Parent)
        {
            IPerception<TSense> perception = base.DeepCopy(Parent) as IPerception<TSense>;

            return perception;
        }

        public override int GetBonusBaseDieRoll()
            => 0;

        public override int GetBonusBaseRadius()
            => 0;

        public override int GetBonusDieRoll()
            => 0;

        public override int GetBonusRadius()
            => 0;

        public override List<Cell> GetRadiusAreaCells()
            => base.GetRadiusAreaCells();

        #endregion

        protected sealed override Type GetSenseType()
            => typeof(TSense);

        public override int Roll(GameObject Entity, bool UseLastRoll = false)
            => base.Roll(Entity, UseLastRoll);

        public override int RollAdvantage(GameObject Entity, bool AgainstLastRoll = false)
            => base.RollAdvantage(Entity, AgainstLastRoll);

        public override int RollDisadvantage(GameObject Entity, bool AgainstLastRoll = false)
            => RollDisadvantage(Entity, AgainstLastRoll);

        #region Event Handling

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            // || ID == EnteredCellEvent.ID
            ;
        public override bool HandleEvent(EnteredCellEvent E)
        {
            // placeholder of the base
            return base.HandleEvent(E);
        }

        #endregion
        #region Comparison

        public int CompareTo(IPerception<TSense> Other)
            => base.CompareTo(Other);

        #endregion
        #region Operator Overloads

        #region Comparison

        public static bool operator <(IPerception<TSense> Op1, IPerception<TSense> Op2)
            => Op1.CompareTo(Op2) < 0;

        public static bool operator >(IPerception<TSense> Op1, IPerception<TSense> Op2)
            => Op1.CompareTo(Op2) > 0;

        public static bool operator <=(IPerception<TSense> Op1, IPerception<TSense> Op2)
            => Op1.CompareTo(Op2) <= 0;

        public static bool operator >=(IPerception<TSense> Op1, IPerception<TSense> Op2)
            => Op1.CompareTo(Op2) >= 0;

        #endregion
        #endregion
    }
}
