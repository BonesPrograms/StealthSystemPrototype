using System;
using System.Collections.Generic;
using System.Text;

using XRL.World;

namespace StealthSystemPrototype
{
    [Serializable]
    public abstract class GameObjectSpecification : Specification<GameObject>
    {
        public GameObjectSpecification(GameObject Subject)
            : base(Subject) { }

        public override void Dispose()
        {
            _Subject = null;
        }

        #region Serialization

        public override void Write(SerializationWriter Writer)
        {
            Writer.WriteGameObject(Subject);
        }

        public override void Read(SerializationReader Reader)
        {
            Subject = Reader.ReadGameObject();
        }

        #endregion

        public static implicit operator bool(GameObjectSpecification Operand)
            => Operand.Check();
    }
}
