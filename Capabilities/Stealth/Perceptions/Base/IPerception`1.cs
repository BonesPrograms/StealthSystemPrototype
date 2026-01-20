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
    public class IPerception<T>
        : IPerception,
        IComparable<IPerception<T>>
        where T : ISense, new()
    {
        #region Helpers

        public class TypedRatingComparer : IComparer<IPerception<T>>
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

            public virtual int Compare(IPerception<T> x, IPerception<T> y)
            {
                if (EitherNull(x, y, out int comparison))
                    return comparison;

                if (Entity!= null)
                {
                    AwarenessLevel awarenessX = x.GetAwareness(Entity, out int rollX);
                    AwarenessLevel awarenessY = y.GetAwareness(Entity, out int rollY);

                    int awarenessComp = awarenessX.CompareTo(awarenessY);
                    if (awarenessComp != 0)
                        return awarenessComp;

                    int rollComp = rollX.CompareTo(rollY);
                    if (rollComp != 0)
                        return rollComp;
                }
                return x.CompareTo(y);
            }
        }

        #endregion
        #region Const & Static Values

        // Add as necessary.

        #endregion
        #region Instance Fields & Properties

        public Type Sense => typeof(T);

        public override ClampedDieRoll DieRoll => _DieRoll ??= GetDieRoll(this, new T());
        public override Radius Radius => _Radius ??= GetRadius(this, new T());

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

        public virtual void FinalizeRead(SerializationReader Reader)
        {
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
            IPerception perception = (IPerception)Activator.CreateInstance(GetType());

            FieldInfo[] fields = GetType().GetFields();

            foreach (FieldInfo fieldInfo in fields)
                if ((fieldInfo.Attributes & FieldAttributes.NotSerialized) == 0
                    && !fieldInfo.IsLiteral)
                    fieldInfo.SetValue(perception, fieldInfo.GetValue(this));

            perception.Owner = Parent;

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

        #endregion

        public virtual List<Cell> GetRadiusAreaCells()
            => Radius.IsArea()
                && Owner?.CurrentCell?.GetCellsInACosmeticCircle(Radius) is IEnumerable<Cell> cells
            ? Event.NewCellList(cells)
            : Event.NewCellList();

        public bool CheckInRadius(GameObject Entity, out int Distance, out FindPath PerceptionPath)
        {
            using Indent indent = new(1);
            Debug.LogMethod(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            PerceptionPath = null;
            Distance = default;

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(Roll) + " requires a " + nameof(GameObject) + " to perceive.");

            if (Entity?.CurrentCell is not Cell { InActiveZone: true } entityCell)
            {
                Debug.CheckNah(nameof(Entity), "Not in active zone", Indent: indent[1]);
                return false;
            }

            if (Owner?.CurrentCell is not Cell { InActiveZone: true } myCell)
            {
                Debug.CheckNah(nameof(Owner), "Not in active zone", Indent: indent[1]);
                return false;
            }

            bool any = false;
            Distance = entityCell.CosmeticDistanceto(myCell.Location);
            int radiusValue = Radius.EffectiveValue;

            if (Radius.IsLine())
            {
                if (!Occludes
                    || entityCell.HasLOSTo(myCell))
                    any = radiusValue >= Distance || any;
                else
                    Debug.CheckNah(
                        Message: Owner.MiniDebugName() +
                            " does not have LOS to " +
                            Entity.MiniDebugName(),
                        Indent: indent[1]);
            }
            if (Radius.IsArea())
            {
                if (RadiusAreaCells.Contains(entityCell))
                {
                    if (!Occludes
                        || entityCell.HasLOSTo(myCell))
                        any = true;
                    else
                        Debug.CheckNah(
                            Message: "Area around " +
                                Owner.MiniDebugName() +
                                " contains " +
                                Entity.MiniDebugName() +
                                ", but does not have LOS",
                            Indent: indent[1]);
                }
                else
                    Debug.CheckNah(
                        Message: "Area around " + 
                            Owner.MiniDebugName() +
                            " does not contain " +
                            Entity.MiniDebugName(),
                        Indent: indent[1]);
            }
            if (Radius.IsPathing())
            {
                PerceptionPath = new(myCell, entityCell);
                if (PerceptionPath.Steps is List<Cell> pathSteps)
                {
                    if (pathSteps.Count >= radiusValue)
                        any = true;
                    else
                        Debug.CheckNah(
                            Message: "Perception path from " +
                                Owner.MiniDebugName() + 
                                " is too long to reach " +
                                Entity.MiniDebugName() ,
                            Indent: indent[1]);
                }
            }
            Debug.YehNah(nameof(any), any, any, Indent: indent[1]);
            return any;
        }

        public virtual int Roll(GameObject Entity, bool UseLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (!CheckInRadius(Entity, out int distance, out FindPath perceptionPath))
                return 0;

            if (UseLastRoll
                && Entity.ID == LastEntityID
                && LastRoll is int lastRoll)
                return lastRoll;

            int roll = DieRoll.Roll();

            LastRoll = roll;
            LastEntityID = Entity.ID;

            double diffusion = Radius.GetDiffusion(distance);

            roll = (int)Math.Floor(roll * Radius.GetDiffusion(distance));

            Debug.Log(nameof(roll), roll, Indent: indent[1]);
            Debug.Log(nameof(distance), distance, Indent: indent[1]);

            string diffussesString = Diffuses.ToString() + ", " + diffusion.WithDigits(3);
            string diffusionCountString = distance.Clamp(new(Radius.GetValue())) + "/" + (Radius.Diffusions()?.Count() ?? 0);
            Debug.Log(nameof(Diffuses), diffussesString + " (" + diffusionCountString + ")", Indent: indent[1]);
            Debug.Log(Radius.GetDiffusionDebug(Inline: false), Indent: indent[2]);

            Debug.Log(nameof(DieRoll), DieRoll, Indent: indent[1]);

            return roll;
        }
        public virtual int RollAdvantage(GameObject Entity, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            GetMinMax(out _, out int max, Roll(Entity, AgainstLastRoll), Roll(Entity, false));
            return max;
        }
        public virtual int RollDisadvantage(GameObject Entity, bool AgainstLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(AgainstLastRoll), AgainstLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            GetMinMax(out int min, out _, Roll(Entity, AgainstLastRoll), Roll(Entity, false));
            return min;
        }

        public static AwarenessLevel CalculateAwareness(int Roll)
            => (AwarenessLevel)((int)Math.Ceiling(((Roll + 1) / 20.0) - 1)).Clamp(0, 4);

        public virtual AwarenessLevel GetAwareness(GameObject Entity, out int Roll, bool UseLastRoll = false)
        {
            using Indent indent = new(1);
            Debug.LogCaller(indent,
                ArgPairs: new Debug.ArgPair[]
                {
                    Debug.Arg(nameof(UseLastRoll), UseLastRoll),
                    Debug.Arg(nameof(Owner), Owner?.DebugName ?? "null"),
                    Debug.Arg(nameof(Entity), Entity?.DebugName ?? "null"),
                });

            if (Entity == null)
                throw new ArgumentNullException(nameof(Entity), nameof(GetAwareness) + " requires a " + nameof(GameObject) + " to perceive.");

            if (UseLastRoll
                && LastRoll != null
                && LastEntityID == Entity.ID)
                Roll = LastRoll.Value;
            else
                Roll = this.Roll(Entity);

            AwarenessLevel awarenessLevel = CalculateAwareness(Roll);

            Debug.CheckYeh(awarenessLevel.ToStringWithNum(), Indent: indent[1]);

            return awarenessLevel;
        }

        public virtual AwarenessLevel GetAwareness(GameObject Entity, bool UseLastRoll = false)
            => GetAwareness(Entity, out _);

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

        public int CompareTo(IPerception<T> Other)
            => base.CompareTo(Other);

        #endregion
        #region Operator Overloads

        #region Comparison

        public static bool operator <(IPerception<T> Op1, IPerception<T> Op2)
            => Op1.CompareTo(Op2) < 0;

        public static bool operator >(IPerception<T> Op1, IPerception<T> Op2)
            => Op1.CompareTo(Op2) > 0;

        public static bool operator <=(IPerception<T> Op1, IPerception<T> Op2)
            => Op1.CompareTo(Op2) <= 0;

        public static bool operator >=(IPerception<T> Op1, IPerception<T> Op2)
            => Op1.CompareTo(Op2) >= 0;

        #endregion
        #endregion
    }
}
