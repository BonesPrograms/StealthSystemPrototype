using System;

using XRL.World;

using StealthSystemPrototype;
using StealthSystemPrototype.Events;
using StealthSystemPrototype.Perceptions;
using StealthSystemPrototype.Capabilities.Stealth;
using StealthSystemPrototype.Logging;
using StealthSystemPrototype.Senses;

namespace StealthSystemPrototype.Perceptions
{
    [Serializable]
    public class SimpleVisualPerception : SimplePerception<Visual>
    {
        #region Constructors

        public SimpleVisualPerception()
            : base()
        {
        }
        public SimpleVisualPerception(GameObject Owner, ClampedDieRoll BaseDieRoll, Purview BaseRadius)
            : base(Owner, BaseDieRoll, BaseRadius)
        {
        }
        public SimpleVisualPerception(GameObject Owner)
            : this(Owner, BASE_DIE_ROLL, new(BASE_RADIUS, VisualFlag))
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

        #endregion

        public override bool WantEvent(int ID, int Cascade)
            => base.WantEvent(ID, Cascade)
            // || ID == EnteredCellEvent.ID
            // || ID == EquippedEvent.ID
            ;
        public override bool HandleEvent(EnteredCellEvent E)
        {
            if (E.Actor?.IsPlayer() ?? false)
            {
                using Indent indent = new(1);
                Debug.LogCaller(indent,
                    ArgPairs: new Debug.ArgPair[]
                    {
                        Debug.Arg(E?.TypeStringWithGenerics()),
                        Debug.Arg(E?.Actor?.DebugName ?? "no one??"),
                    });
            }
            return base.HandleEvent(E);
        }
        public override bool HandleEvent(EquippedEvent E)
        {
            if (E.Actor?.IsPlayer() ?? false)
            {
                using Indent indent = new(1);
                Debug.LogCaller(indent,
                    ArgPairs: new Debug.ArgPair[]
                    {
                        Debug.Arg(E?.TypeStringWithGenerics()),
                        Debug.Arg(E?.Actor?.DebugName ?? "no one??"),
                    });
            }
            return base.HandleEvent(E);
        }
    }
}
